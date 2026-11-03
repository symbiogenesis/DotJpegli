namespace DotJpegli.Performance;

using System.Diagnostics;
using System.Reflection;
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
        int Gen1Collections, int Gen2Collections, string JpegSha256, long JpegBytes);
    internal sealed record Request(string Kind, string? Input = null, int Sampling = 0, int Progression = 0,
        float Distance = 0, int Iterations = 0, int Warmups = 0);
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
                // Keep the input, immutable settings and expected JPEG alive between legs.
                // Parsing, image I/O and first-use preparation are outside the timer.
                if (previous is null || request.Input != previous.Input || request.Sampling != previous.Sampling
                    || request.Progression != previous.Progression || request.Distance != previous.Distance)
                {
                    image = PpmImage.Read(request.Input!);
                    settings = new()
                    {
                        ColorSpace = JpegColorSpace.YCbCr,
                        Distance = request.Distance,
                        ProgressiveLevel = request.Progression,
                        Subsampling = request.Sampling == 444 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
                        OptimizeHuffmanTables = true,
                        AdaptiveQuantization = true,
                        ForceBaseline = true,
                        MaxDegreeOfParallelism = 1
                    };
                    expected = Encode(image, settings);
                }

                previous = request;
                await Respond(new("measurement", Measurement: Measure(image!, settings!, expected!,
                    request.Iterations, request.Warmups)));
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

    private static void Validate(Request request)
    {
        if (request.Kind != "measure" || string.IsNullOrWhiteSpace(request.Input)
            || !Path.IsPathFullyQualified(request.Input) || request.Sampling is not (444 or 420)
            || request.Progression is not (0 or 2) || request.Distance is not (1 or 4)
            || request.Iterations is < 1 or > 100 || request.Warmups is < 0 or > 100)
        {
            throw new InvalidDataException("Invalid revision-worker measurement request.");
        }
    }

    private static Measurement Measure(PpmImage image, JpegliEncoderOptions settings, byte[] expected,
        int iterations, int warmups)
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
            Convert.ToHexStringLower(SHA256.HashData(expected)), expected.LongLength);
    }

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

        var measurement = new Measurement(1_500_000, 4096, 1, 2, 3, new string('a', 64), 123);
        var response = new Response("measurement", Measurement: measurement);
        if (JsonSerializer.Deserialize<Response>(JsonSerializer.Serialize(response)) != response)
        {
            throw new InvalidDataException("Revision-worker measurement response round trip failed.");
        }

        foreach (Request invalid in new Request[]
        {
            request with { Kind = "unknown" }, request with { Input = "relative.ppm" },
            request with { Sampling = 422 }, request with { Progression = 1 }, request with { Distance = float.NaN },
            request with { Iterations = 0 }, request with { Iterations = 101 }, request with { Warmups = -1 }
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
        float distance, int iterations, int warmups)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(10));
        try
        {
            await this.Write(new("measure", input, sampling, progression, distance, iterations, warmups), timeout.Token);
            RevisionWorker.Response response = await this.Read(timeout.Token);
            return response.Kind == "measurement" && response.Measurement is { } measurement ? measurement
                : throw new InvalidDataException("Revision worker returned an unexpected response.");
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
