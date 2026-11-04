namespace DotJpegli.Performance;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using DotJpegli.Parity;

/// <summary>Persistent CI-only encoder process used by the revision comparison.</summary>
internal static class RevisionWorker
{
    private static readonly JsonSerializerOptions EnvironmentJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    internal sealed record Identity(string EncoderSha256, string RunnerSha256, string TestedRevision,
        JsonElement Environment, string AssemblyInformationalVersion);
    internal sealed record Measurement(double NsPerEncode, long AllocatedBytes, int Gen0Collections,
        int Gen1Collections, int Gen2Collections, string JpegSha256, long JpegBytes, bool SourceIcc = false,
        string? SourceIccProfileSha256 = null, string Scenario = "ordinary", string? TargetIccProfileSha256 = null);
    internal sealed record Request(string Kind, string? Input = null, int Sampling = 0, int Progression = 0,
        float Distance = 0, int Iterations = 0, int Warmups = 0, bool SourceIcc = false, string Scenario = "ordinary");
    internal sealed record Response(string Kind, Identity? Identity = null, Measurement? Measurement = null,
        string? Error = null);

    internal static async Task<int> Run()
    {
        Program.RequireCi();
        try
        {
            string revision = Environment.GetEnvironmentVariable("DOTJPEGLI_PERFORMANCE_REVISION")
                ?? throw new InvalidDataException("Revision workers require an explicit tested revision.");
            Assembly encoder = typeof(JpegliEncoder).Assembly;
            var identity = new Identity(HashFile(encoder.Location), HashFile(Assembly.GetExecutingAssembly().Location),
                revision, JsonSerializer.SerializeToElement(Program.EnvironmentDetails(), EnvironmentJsonOptions),
                encoder.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? throw new InvalidDataException("The encoder has no informational version."));
            await Respond(new("identity", Identity: identity));

            Request? previous = null;
            PpmImage? image = null;
            JpegliEncoderOptions? settings = null;
            byte[]? expected = null;
            while (await Console.In.ReadLineAsync() is { } line)
            {
                Request request = JsonSerializer.Deserialize<Request>(line)
                    ?? throw new InvalidDataException("Missing revision-worker request.");
                if (request.Kind == "shutdown")
                {
                    await Respond(new("shutdown"));
                    return 0;
                }

                Validate(request);
                if (request.Scenario == "parallel-two" && Environment.ProcessorCount < 2)
                {
                    throw new InvalidDataException("Parallel revision measurements require at least two available CPUs.");
                }
                // Keep the input, immutable settings and expected JPEG alive between legs.
                // Parsing, image I/O and first-use preparation are outside the timer.
                if (previous is null || !SameInputAndSettings(previous, request))
                {
                    image = PpmImage.Read(request.Input!);
                    settings = CreateSettings(request);
                    expected = Encode(image, settings);
                }

                previous = request;
                await Respond(new("measurement", Measurement: Measure(image!, settings!, expected!,
                    request.Iterations, request.Warmups, request.Scenario)));
            }

            return 0;
        }
        catch (Exception error) when (error is ArgumentException or IOException or InvalidDataException
            or InvalidOperationException or JsonException or OverflowException or UnauthorizedAccessException)
        {
            await Respond(new("error", Error: error.Message));
            return 2;
        }
    }

    private static bool SameInputAndSettings(Request first, Request second)
        => first.Input == second.Input && first.Sampling == second.Sampling && first.Progression == second.Progression
            && first.Distance == second.Distance && first.SourceIcc == second.SourceIcc && first.Scenario == second.Scenario;

    private static JpegliEncoderOptions CreateSettings(Request request)
    {
        var options = new JpegliEncoderOptions
        {
            ColorSpace = JpegColorSpace.YCbCr,
            Distance = request.Distance,
            ProgressiveLevel = request.Progression,
            Subsampling = request.Sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
            OptimizeHuffmanTables = true,
            AdaptiveQuantization = true,
            ForceBaseline = true,
            MaxDegreeOfParallelism = 1,
            SourceIccProfile = request.SourceIcc ? JpegliIccProfiles.CreateDisplayP3Profile() : ReadOnlyMemory<byte>.Empty
        };
        return request.Scenario == "ordinary" ? options : ApplyScenario(options, request.Scenario);
    }

    // Keep references to newer optional APIs out of the released-version JIT
    // path. The identical worker must still load the historical 1.0.0 encoder.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static JpegliEncoderOptions ApplyScenario(JpegliEncoderOptions options, string scenario) => scenario switch
    {
        "smoothing" => options with { SmoothingFactor = 50 },
        // Subsampling presets are ignored for RGB/XYB. Match the numeric labels
        // explicitly: conventional 4:2:0 subsamples XYB's Y (component1), which
        // disables AQ under the encoder's existing analysis-component policy.
        "xyb" => options with
        {
            ColorSpace = JpegColorSpace.Xyb,
            IccProfile = JpegliIccProfiles.CreateXybProfile(),
            SamplingFactors = options.Subsampling == ChromaSubsampling.Quarter
                ? new(new(2, 2), new(1, 1), new(1, 1)) : new(new(1, 1), new(1, 1), new(1, 1))
        },
        "cache-constrained" => options with { MaxCacheBytes = 4L * 1024 * 1024 },
        "sampled-icc" => options with { IccProfile = SampledIccScenario.CreateProfile() },
        "parallel-two" => options with { MaxDegreeOfParallelism = 2 },
        _ => throw new InvalidDataException("Unknown revision-worker scenario.")
    };

    private static void Validate(Request request)
    {
        if (request.Kind != "measure" || string.IsNullOrWhiteSpace(request.Input)
            || !Path.IsPathFullyQualified(request.Input) || request.Sampling is not (444 or 420)
            || request.Progression is not (0 or 2) || request.Distance is not (1 or 4)
            || request.Iterations is < 1 or > 100 || request.Warmups is < 0 or > 100
            || request.Scenario is not ("ordinary" or "smoothing" or "xyb" or "cache-constrained" or "sampled-icc" or "parallel-two")
            || (request.SourceIcc && request.Scenario is not ("ordinary" or "sampled-icc"))
            || (request.Scenario == "sampled-icc" && !request.SourceIcc))
        {
            throw new InvalidDataException("Invalid revision-worker measurement request.");
        }
    }

    private static Measurement Measure(PpmImage image, JpegliEncoderOptions settings, byte[] expected,
        int iterations, int warmups, string scenario)
    {
        for (int index = 0; index < warmups; index++)
        {
            RequireSame(expected, Encode(image, settings));
        }

        int gen0 = GC.CollectionCount(0), gen1 = GC.CollectionCount(1), gen2 = GC.CollectionCount(2);
        long allocated = GC.GetTotalAllocatedBytes(precise: true), ticks = 0;
        for (int index = 0; index < iterations; index++)
        {
            long start = Stopwatch.GetTimestamp();
            byte[] jpeg = Encode(image, settings);
            ticks += Stopwatch.GetTimestamp() - start;
            RequireSame(expected, jpeg);
            GC.KeepAlive(jpeg);
        }

        long allocatedBytes = GC.GetTotalAllocatedBytes(precise: true) - allocated;
        return new(ticks * (1_000_000_000.0 / Stopwatch.Frequency) / iterations, allocatedBytes,
            GC.CollectionCount(0) - gen0, GC.CollectionCount(1) - gen1, GC.CollectionCount(2) - gen2,
            Convert.ToHexStringLower(SHA256.HashData(expected)), expected.LongLength, !settings.SourceIccProfile.IsEmpty,
            settings.SourceIccProfile.IsEmpty ? null : Convert.ToHexStringLower(SHA256.HashData(settings.SourceIccProfile.Span)),
            scenario, settings.IccProfile.IsEmpty ? null : Convert.ToHexStringLower(SHA256.HashData(settings.IccProfile.Span)));
    }

    internal static void ValidateMeasurementMode(Measurement measurement, bool sourceIcc, string scenario = "ordinary")
    {
        string? expectedProfile = sourceIcc ? Convert.ToHexStringLower(SHA256.HashData(JpegliIccProfiles.CreateDisplayP3Profile())) : null;
        string? expectedTarget = scenario == "sampled-icc" ? Convert.ToHexStringLower(SHA256.HashData(SampledIccScenario.CreateProfile()))
            : scenario == "xyb" ? XybProfileHash() : null;
        if (measurement.SourceIcc != sourceIcc || measurement.SourceIccProfileSha256 != expectedProfile
            || measurement.Scenario != scenario || measurement.TargetIccProfileSha256 != expectedTarget)
        {
            throw new InvalidDataException("Revision-worker measurement color conversion or source profile does not match its request.");
        }
    }

    // Keep optional profile APIs out of the ordinary released-worker JIT path.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string XybProfileHash() => Convert.ToHexStringLower(SHA256.HashData(JpegliIccProfiles.CreateXybProfile()));

    private static byte[] Encode(PpmImage image, JpegliEncoderOptions settings)
        => JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24, settings);
    private static void RequireSame(byte[] expected, byte[] actual)
    {
        if (!expected.AsSpan().SequenceEqual(actual))
        {
            throw new InvalidDataException("Revision-worker JPEG output is not deterministic.");
        }
    }

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }

    private static async Task Respond(Response response)
    {
        await Console.Out.WriteLineAsync(JsonSerializer.Serialize(response));
        await Console.Out.FlushAsync();
    }

    internal static void SelfTest()
    {
        var request = new Request("measure", Path.GetFullPath("synthetic input.ppm"), 420, 2, 4, 3, 5);
        Request roundTrip = JsonSerializer.Deserialize<Request>(JsonSerializer.Serialize(request))!;
        Validate(roundTrip);
        if (roundTrip != request)
        {
            throw new InvalidDataException("Revision-worker request round trip failed.");
        }
        Request iccRequest = request with { SourceIcc = true };
        Request iccRoundTrip = JsonSerializer.Deserialize<Request>(JsonSerializer.Serialize(iccRequest))!;
        Validate(iccRoundTrip);
        Request legacy = JsonSerializer.Deserialize<Request>("{\"Kind\":\"measure\",\"Input\":null}")!;
        if (iccRoundTrip != iccRequest || legacy.SourceIcc
            || SameInputAndSettings(request, iccRequest)
            || !SameInputAndSettings(request, request with { Warmups = 10, Iterations = 100 }))
        {
            throw new InvalidDataException("Revision-worker color mode compatibility or settings reuse failed.");
        }
        JpegliEncoderOptions ordinary = CreateSettings(request), managed = CreateSettings(iccRequest);
        if (!ordinary.SourceIccProfile.IsEmpty || managed.SourceIccProfile.IsEmpty || !managed.IccProfile.IsEmpty
            || managed.ColorSpace != JpegColorSpace.YCbCr || managed.MaxDegreeOfParallelism != 1
            || !managed.SourceIccProfile.Span.SequenceEqual(JpegliIccProfiles.CreateDisplayP3Profile()))
        {
            throw new InvalidDataException("Revision-worker source ICC policy changed.");
        }

        var measurement = new Measurement(1_500_000, 4096, 1, 2, 3, new string('a', 64), 123);
        var response = new Response("measurement", Measurement: measurement);
        if (JsonSerializer.Deserialize<Response>(JsonSerializer.Serialize(response)) != response)
        {
            throw new InvalidDataException("Revision-worker measurement response round trip failed.");
        }
        ValidateMeasurementMode(measurement, sourceIcc: false);
        SampledIccScenario.SelfTest();
        var tiny = new PpmImage(2, 2, [0, 0, 0, 255, 255, 255, 255, 0, 32, 17, 191, 251]);
        CheckTinyEncode(tiny, ordinary);
        CheckTinyEncode(tiny, managed);
        foreach (string scenario in new[] { "smoothing", "xyb", "cache-constrained", "sampled-icc", "parallel-two" })
        {
            foreach (int sampling in new[] { 444, 420 })
            {
                bool sampled = scenario == "sampled-icc";
                Request scenarioRequest = request with { Scenario = scenario, SourceIcc = sampled, Sampling = sampling };
                Validate(scenarioRequest);
                if (SameInputAndSettings(request, scenarioRequest)
                    || JsonSerializer.Deserialize<Request>(JsonSerializer.Serialize(scenarioRequest)) != scenarioRequest)
                {
                    throw new InvalidDataException("Revision-worker scenario settings must remain distinct and round-trip.");
                }
                JpegliEncoderOptions selected = CreateSettings(scenarioRequest);
                if ((scenario == "smoothing" && selected.SmoothingFactor != 50)
                    || (scenario == "xyb" && (selected.ColorSpace != JpegColorSpace.Xyb || !selected.SourceIccProfile.IsEmpty
                        || !selected.IccProfile.Span.SequenceEqual(JpegliIccProfiles.CreateXybProfile())))
                    || (scenario == "cache-constrained" && selected.MaxCacheBytes != 4L * 1024 * 1024)
                    || (sampled && (selected.IccProfile.IsEmpty || selected.SourceIccProfile.IsEmpty))
                    || (scenario == "parallel-two" && selected.MaxDegreeOfParallelism != 2))
                {
                    throw new InvalidDataException("Revision-worker scenario was not applied.");
                }
                ValidateMeasurementMode(measurement with
                {
                    Scenario = scenario,
                    SourceIcc = sampled,
                    SourceIccProfileSha256 = sampled ? Convert.ToHexStringLower(SHA256.HashData(selected.SourceIccProfile.Span)) : null,
                    TargetIccProfileSha256 = selected.IccProfile.IsEmpty ? null : Convert.ToHexStringLower(SHA256.HashData(selected.IccProfile.Span))
                }, sampled, scenario);
                CheckTinyEncode(tiny, selected);
            }
        }
        Measurement iccMeasurement = measurement with
        {
            SourceIcc = true,
            SourceIccProfileSha256 = Convert.ToHexStringLower(SHA256.HashData(managed.SourceIccProfile.Span))
        };
        ValidateMeasurementMode(iccMeasurement, sourceIcc: true);
        foreach (Measurement mismatched in new[] { measurement, iccMeasurement with { SourceIccProfileSha256 = new string('b', 64) } })
        {
            try
            {
                ValidateMeasurementMode(mismatched, sourceIcc: true);
                throw new InvalidOperationException("Revision-worker measurement accepted a mismatched color mode or source profile.");
            }
            catch (InvalidDataException)
            {
                // An old or mismatched worker must not silently measure another mode.
            }
        }

        foreach (Request invalid in new Request[]
        {
            request with { Kind = "unknown" }, request with { Input = "relative.ppm" },
            request with { Sampling = 422 }, request with { Progression = 1 }, request with { Distance = float.NaN },
            request with { Iterations = 0 }, request with { Iterations = 101 }, request with { Warmups = -1 },
            request with { Scenario = "unknown" }, request with { Scenario = "xyb", SourceIcc = true },
            request with { Scenario = "sampled-icc", SourceIcc = false }
        })
        {
            try
            {
                Validate(invalid);
            }
            catch (InvalidDataException)
            {
                continue;
            }

            throw new InvalidDataException("Revision-worker accepted invalid synthetic input.");
        }
        Console.WriteLine("Revision-worker scenario correctness checks passed (tiny encodes; no clocks).");
    }

    private static void CheckTinyEncode(PpmImage image, JpegliEncoderOptions settings)
    {
        byte[] jpeg = Encode(image, settings);
        if (jpeg.Length < 4 || jpeg[0] != 0xff || jpeg[1] != 0xd8 || jpeg[^2] != 0xff || jpeg[^1] != 0xd9)
        {
            throw new InvalidDataException("A revision scenario did not produce a complete tiny JPEG.");
        }
        var structure = JpegStructure.Read(jpeg);
        bool progressive = settings.ProgressiveLevel is { } level ? level > 0 : settings.Progressive;
        if (structure.Width != image.Width || structure.Height != image.Height || structure.Components.Length != 3
            || structure.FrameMarker != (progressive ? 0xc2 : 0xc0))
        {
            throw new InvalidDataException("A revision scenario did not retain its requested frame settings.");
        }
        for (int component = 0; component < 3; component++)
        {
            int expected = component == 0 && settings.Subsampling == ChromaSubsampling.Quarter ? 2 : 1;
            if (structure.Components[component].HorizontalSampling != expected || structure.Components[component].VerticalSampling != expected)
            {
                throw new InvalidDataException("A revision scenario did not retain its requested numeric sampling factors.");
            }
        }
        RequireSame(jpeg, Encode(image, settings));
    }
}

/// <summary>Owns one persistent revision worker and its JSON-lines protocol.</summary>
internal sealed class RevisionWorkerClient : IAsyncDisposable
{
    private readonly Process process;
    private readonly Task<string> standardError;
    private bool disposed;

    private RevisionWorkerClient(Process process)
    {
        this.process = process;
        this.standardError = process.StandardError.ReadToEndAsync();
    }

    internal RevisionWorker.Identity Identity { get; private set; } = null!;

    internal static async Task<RevisionWorkerClient> Start(string runnerPath, string revision)
    {
        var start = new ProcessStartInfo(Environment.ProcessPath
            ?? throw new InvalidOperationException("The current dotnet host path is unavailable."))
        {
            WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(runnerPath)),
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        start.ArgumentList.Add(Path.GetFullPath(runnerPath));
        start.ArgumentList.Add("--revision-worker");
        start.Environment["DOTJPEGLI_PERFORMANCE_REVISION"] = revision;
        var client = new RevisionWorkerClient(Process.Start(start)
            ?? throw new InvalidOperationException("The revision worker could not start."));
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            RevisionWorker.Response response = await client.Read(timeout.Token);
            client.Identity = response.Kind == "identity" && response.Identity is { } identity
                && identity.TestedRevision == revision
                && identity.AssemblyInformationalVersion.EndsWith("+" + revision, StringComparison.Ordinal) ? identity
                : throw new InvalidDataException("Revision-worker handshake has an unexpected identity.");
            return client;
        }
        catch (OperationCanceledException error)
        {
            await client.Stop();
            client.process.Dispose();
            throw new TimeoutException("Revision-worker startup exceeded the one-minute limit.", error);
        }
        catch
        {
            await client.Stop();
            client.process.Dispose();
            throw;
        }
    }

    internal async Task<RevisionWorker.Measurement> Measure(string input, int sampling, int progression,
        float distance, int iterations, int warmups, bool sourceIcc = false, string scenario = "ordinary")
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        try
        {
            await this.Write(new("measure", input, sampling, progression, distance, iterations, warmups, sourceIcc, scenario), timeout.Token);
            RevisionWorker.Response response = await this.Read(timeout.Token);
            if (response.Kind != "measurement" || response.Measurement is not { } measurement)
            {
                throw new InvalidDataException("Revision worker returned an unexpected response.");
            }
            RevisionWorker.ValidateMeasurementMode(measurement, sourceIcc, scenario);
            return measurement;
        }
        catch (OperationCanceledException error)
        {
            await this.Stop();
            throw new TimeoutException("Revision-worker measurement exceeded the ten-minute limit.", error);
        }
    }

    private async Task Write(RevisionWorker.Request request, CancellationToken cancellationToken)
    {
        await this.process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request).AsMemory(), cancellationToken);
        await this.process.StandardInput.FlushAsync(cancellationToken);
    }

    private async Task<RevisionWorker.Response> Read(CancellationToken cancellationToken)
    {
        string? line = await this.process.StandardOutput.ReadLineAsync(cancellationToken);
        if (line is null)
        {
            await this.process.WaitForExitAsync(cancellationToken);
            throw new InvalidDataException($"Revision worker exited with code {this.process.ExitCode}: {await this.standardError}");
        }

        RevisionWorker.Response response = JsonSerializer.Deserialize<RevisionWorker.Response>(line)
            ?? throw new InvalidDataException("Revision worker returned an empty response.");
        if (response.Error is { } error)
        {
            throw new InvalidDataException($"Revision worker failed: {error}");
        }

        return response;
    }

    private async Task Stop()
    {
        if (!this.process.HasExited)
        {
            try
            {
                this.process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) when (this.process.HasExited)
            {
                // The child can exit between HasExited and Kill.
            }
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try
        {
            await this.process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException error)
        {
            throw new TimeoutException("Revision worker did not terminate within fifteen seconds.", error);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        try
        {
            if (!this.process.HasExited)
            {
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                await this.Write(new("shutdown"), timeout.Token);
                RevisionWorker.Response response = await this.Read(timeout.Token);
                if (response.Kind != "shutdown")
                {
                    throw new InvalidDataException("Revision worker did not acknowledge shutdown.");
                }

                this.process.StandardInput.Close();
                await this.process.WaitForExitAsync(timeout.Token);
            }

            if (this.process.ExitCode != 0)
            {
                throw new InvalidDataException($"Revision worker exited with code {this.process.ExitCode}: {await this.standardError}");
            }
        }
        catch (OperationCanceledException error)
        {
            throw new TimeoutException("Revision-worker shutdown exceeded the fifteen-second limit.", error);
        }
        finally
        {
            try
            {
                await this.Stop();
            }
            finally
            {
                this.process.Dispose();
            }
        }
    }
}
