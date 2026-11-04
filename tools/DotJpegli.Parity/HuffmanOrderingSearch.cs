namespace DotJpegli.Parity;

using System.Collections.ObjectModel;
using System.Security.Cryptography;
using DotJpegli.Internal;

// Complete-file ranking includes stuffing and padding, unlike histogram bits.
internal static class HuffmanOrderingSearch
{
    internal sealed record Candidate(string Id, HuffmanSymbolOrder Order)
    {
        internal JpegliEncoderOptions CreateOptions(float distance, ChromaSubsampling sampling, int progression) => new()
        {
            ColorSpace = JpegColorSpace.YCbCr,
            Distance = distance,
            Subsampling = sampling,
            ProgressiveLevel = progression,
            OptimizeHuffmanTables = true,
            AdaptiveQuantization = true,
            ForceBaseline = true,
            MaxDegreeOfParallelism = 1,
            HuffmanSymbolOrder = this.Order
        };
    }

    internal sealed record Trial(string Id, int Bytes, string Sha256, string DecodedPixelSha256,
        int ScanCount, int HuffmanDefinitionBytes);

    internal sealed record SearchResult(byte[] Jpeg, Candidate Winner, IReadOnlyList<Trial> Trials)
    {
        internal int EncodeCount => this.Trials.Count;
        internal int DecodeCount => this.Trials.Count;
        internal int TotalScanCount => this.Trials.Sum(trial => trial.ScanCount);
    }

    internal static ReadOnlyCollection<Candidate> Candidates { get; } = Array.AsReadOnly<Candidate>(
    [
        new("control", HuffmanSymbolOrder.Canonical),
        new("frequency-descending", HuffmanSymbolOrder.FrequencyDescending),
        new("frequency-ascending", HuffmanSymbolOrder.FrequencyAscending),
        new("reverse-within-length", HuffmanSymbolOrder.ReverseWithinLength)
    ]);

    internal static SearchResult Search(PpmImage image, float distance, ChromaSubsampling sampling,
        int progression, Func<byte[], PpmImage> decode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Width is < 1 or > 1024 || image.Height is < 1 or > 1024
            || image.Pixels.Length != image.Width * image.Height * 3)
        {
            throw new ArgumentException("Huffman search accepts complete RGB8 images up to 1024 by 1024 pixels.", nameof(image));
        }
        if (sampling is not (ChromaSubsampling.None or ChromaSubsampling.Quarter))
        {
            throw new ArgumentOutOfRangeException(nameof(sampling));
        }
        if (progression is not (0 or 2))
        {
            throw new ArgumentOutOfRangeException(nameof(progression));
        }
        return Run(candidate => JpegliEncoder.Encode(image.Pixels, image.Width, image.Height, PixelLayout.Rgb24,
            candidate.CreateOptions(distance, sampling, progression), cancellationToken: cancellationToken),
            decode, image.Width, image.Height, cancellationToken);
    }

    internal static SearchResult Run(Func<Candidate, byte[]> encode, Func<byte[], PpmImage> decode,
        int width, int height, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(encode);
        ArgumentNullException.ThrowIfNull(decode);
        var trials = new List<Trial>(Candidates.Count);
        byte[]? best = null, controlPixels = null;
        Candidate winner = Candidates[0];
        foreach (Candidate candidate in Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] jpeg = encode(candidate);
            cancellationToken.ThrowIfCancellationRequested();
            var structure = JpegStructure.Read(jpeg);
            PpmImage decoded = decode(jpeg);
            if (decoded.Width != width || decoded.Height != height || decoded.Pixels.Length != (long)width * height * 3)
            {
                throw new InvalidDataException("Huffman ordering changed decoded dimensions or produced incomplete pixels.");
            }
            controlPixels ??= (byte[])decoded.Pixels.Clone();
            if (!decoded.Pixels.AsSpan().SequenceEqual(controlPixels))
            {
                throw new InvalidDataException("Huffman ordering changed decoded RGB pixels.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            trials.Add(new(candidate.Id, jpeg.Length, Hash(jpeg), Hash(decoded.Pixels),
                structure.Scans.Length, structure.HuffmanDefinitionBytes));
            // Full size, including DHT bytes, stuffing and scan padding, decides.
            // Control is first and remains selected on every complete-size tie.
            if (best is null || jpeg.Length < best.Length)
            {
                best = jpeg;
                winner = candidate;
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(best!, winner, trials.AsReadOnly());
    }

    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
}
