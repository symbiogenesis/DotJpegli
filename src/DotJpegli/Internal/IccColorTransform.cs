using System.Buffers.Binary;

namespace DotJpegli.Internal;

// ICC.1 v2/v4 transforms are connected in unencoded D50 XYZ. See ICC.1:2022
// sections 6.3, 8.10, 10.6, 10.10-10.13, 10.18 and Annex F.
// All parsed arrays are private snapshots; conversion allocates no per-pixel state.
internal sealed class IccColorTransform
{
    private const double D50X = 0.9642;
    private const double D50Z = 0.8249;
    private const double XyzScale = 65535.0 / 32768.0;
    private readonly ProfileTransform source;
    private readonly ProfileTransform destination;
    private readonly Triple absoluteScale;

    private IccColorTransform(ProfileTransform source, ProfileTransform destination, Triple absoluteScale)
    {
        this.source = source;
        this.destination = destination;
        this.absoluteScale = absoluteScale;
    }

    internal static IccColorTransform Create(ReadOnlySpan<byte> sourceProfile,
        ReadOnlySpan<byte> destinationProfile, bool sourceGrayscale, bool destinationGrayscale,
        IccRenderingIntent renderingIntent, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (renderingIntent is < IccRenderingIntent.Perceptual or > IccRenderingIntent.AbsoluteColorimetric)
            throw new ArgumentOutOfRangeException(nameof(renderingIntent));
        Profile input = new(sourceProfile, sourceGrayscale, "SourceIccProfile", cancellationToken);
        Profile output = new(destinationProfile, destinationGrayscale, "IccProfile", cancellationToken);
        ProfileTransform inputTransform = input.ReadTransform(true, renderingIntent);
        ProfileTransform outputTransform = output.ReadTransform(false, renderingIntent);
        Triple scale = new(1, 1, 1);
        if (renderingIntent == IccRenderingIntent.AbsoluteColorimetric)
        {
            Triple sourceWhite = input.ReadWhite();
            Triple destinationWhite = output.ReadWhite();
            scale = new(sourceWhite.X / destinationWhite.X, sourceWhite.Y / destinationWhite.Y,
                sourceWhite.Z / destinationWhite.Z);
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(inputTransform, outputTransform, scale);
    }

    internal void Convert(float red, float green, float blue,
        out float outputRed, out float outputGreen, out float outputBlue)
    {
        if (!float.IsFinite(red) || !float.IsFinite(green) || !float.IsFinite(blue))
            throw new ArgumentException("ICC conversion requires finite normalized samples.", "pixels");
        Triple xyz = source.ToXyz(new(Clamp(red), Clamp(green), Clamp(blue)));
        xyz = new(xyz.X * absoluteScale.X, xyz.Y * absoluteScale.Y, xyz.Z * absoluteScale.Z);
        EnsureFinite(xyz);
        Triple result = destination.FromXyz(xyz);
        EnsureFinite(result);
        outputRed = (float)Clamp(result.X);
        outputGreen = (float)Clamp(result.Y);
        outputBlue = (float)Clamp(result.Z);
    }

    private static double Clamp(double value) => Math.Clamp(value, 0, 1);

    private static void EnsureFinite(Triple value)
    {
        if (!double.IsFinite(value.X) || !double.IsFinite(value.Y) || !double.IsFinite(value.Z))
            throw new ArgumentException("The ICC conversion produced a non-finite color value.", "SourceIccProfile");
    }

    private readonly record struct Triple(double X, double Y, double Z);

    private sealed class ProfileTransform(bool grayscale, bool lab, Curve[]? curves,
        Matrix? matrix, Pipeline? pipeline, bool legacyLab)
    {
        internal Triple ToXyz(Triple device)
        {
            if (pipeline is not null)
                return DecodePcs(pipeline.Apply(device), lab, legacyLab);
            if (grayscale)
            {
                double value = curves![0].Apply(device.X);
                return lab ? LabToXyz(new(100 * value, 0, 0)) : new(D50X * value, value, D50Z * value);
            }
            Triple linear = new(curves![0].Apply(device.X), curves[1].Apply(device.Y), curves[2].Apply(device.Z));
            return matrix!.Apply(linear, false);
        }

        internal Triple FromXyz(Triple xyz)
        {
            if (pipeline is not null)
            {
                Triple result = pipeline.Apply(EncodePcs(xyz, lab, legacyLab));
                return grayscale ? new(result.X, result.X, result.X) : result;
            }
            if (grayscale)
            {
                double value = curves![0].Inverse(lab ? XyzToLab(xyz).X / 100 : xyz.Y);
                return new(value, value, value);
            }
            Triple linear = matrix!.Apply(xyz, false);
            return new(curves![0].Inverse(linear.X), curves[1].Inverse(linear.Y), curves[2].Inverse(linear.Z));
        }
    }

    private static Triple DecodePcs(Triple value, bool lab, bool legacyLab)
    {
        if (!lab)
            return new(value.X * XyzScale, value.Y * XyzScale, value.Z * XyzScale);
        double scale = legacyLab ? 65535.0 / 65280.0 : 1;
        return LabToXyz(new(value.X * scale * 100, value.Y * scale * 255 - 128,
            value.Z * scale * 255 - 128));
    }

    private static Triple EncodePcs(Triple xyz, bool lab, bool legacyLab)
    {
        if (!lab)
            return new(xyz.X / XyzScale, xyz.Y / XyzScale, xyz.Z / XyzScale);
        Triple value = XyzToLab(xyz);
        double scale = legacyLab ? 65280.0 / 65535.0 : 1;
        return new(value.X / 100 * scale, (value.Y + 128) / 255 * scale,
            (value.Z + 128) / 255 * scale);
    }

    private static Triple LabToXyz(Triple lab)
    {
        double fy = (lab.X + 16) / 116;
        return new(D50X * LabInverse(fy + lab.Y / 500), LabInverse(fy),
            D50Z * LabInverse(fy - lab.Z / 200));
    }

    private static Triple XyzToLab(Triple xyz)
    {
        double fx = LabForward(xyz.X / D50X), fy = LabForward(xyz.Y), fz = LabForward(xyz.Z / D50Z);
        return new(116 * fy - 16, 500 * (fx - fy), 200 * (fy - fz));
    }

    private static double LabForward(double value) => value > 216.0 / 24389
        ? Math.Cbrt(value) : (24389.0 / 27 * value + 16) / 116;

    private static double LabInverse(double value) => value > 6.0 / 29
        ? value * value * value : 108.0 / 841 * (value - 4.0 / 29);

    private sealed class Profile
    {
        private readonly byte[] bytes;
        private readonly Dictionary<uint, Tag> tags = [];
        private readonly bool grayscale;
        private readonly bool lab;
        private readonly string parameter;
        private readonly CancellationToken cancellationToken;

        internal Profile(ReadOnlySpan<byte> data, bool grayscale, string parameter,
            CancellationToken cancellationToken)
        {
            this.grayscale = grayscale;
            this.parameter = parameter;
            this.cancellationToken = cancellationToken;
            if (data.Length < 132)
                throw Error("The ICC header or tag table is truncated.");
            uint size = BinaryPrimitives.ReadUInt32BigEndian(data);
            if (size < 132 || size > data.Length || size > int.MaxValue)
                throw Error("The ICC profile size is invalid.");
            bytes = data[..(int)size].ToArray();
            if (Signature(36) != Sig("acsp"))
                throw Error("The ICC signature is missing.");
            if (bytes[8] is not (2 or 4))
                throw Error("Only ICC version 2 and version 4 profiles are supported.");
            uint deviceClass = Signature(12);
            if (deviceClass != Sig("scnr") && deviceClass != Sig("mntr") &&
                deviceClass != Sig("prtr") && deviceClass != Sig("spac"))
                throw Error("The ICC profile class is unsupported; device links, abstract and named-color profiles cannot describe these samples.");
            if (Signature(16) != Sig(grayscale ? "GRAY" : "RGB "))
                throw Error("The ICC device color space does not match the RGB or grayscale samples.");
            uint pcs = Signature(20);
            lab = pcs == Sig("Lab ");
            if (!lab && pcs != Sig("XYZ "))
                throw Error("Only XYZ and Lab ICC profile connection spaces are supported.");
            uint count = UInt32(128);
            if (count > (bytes.Length - 132) / 12)
                throw Error("The ICC tag table is truncated.");
            int tableEnd = 132 + (int)count * 12;
            for (int index = 0; index < count; index++)
            {
                CheckCancellation(index);
                int entry = 132 + index * 12;
                uint offset = UInt32(entry + 4), length = UInt32(entry + 8);
                if (offset < tableEnd || (offset & 3) != 0 || length < 8 || offset > bytes.Length ||
                    length > bytes.Length - offset)
                    throw Error("An ICC tag has an invalid offset or length.");
                if (!tags.TryAdd(Signature(entry), new((int)offset, (int)length)))
                    throw Error("The ICC tag table contains a duplicate signature.");
            }
            // Exact tag sharing is permitted. Partial overlaps are malformed.
            Tag[] ranges = tags.Values.Distinct().OrderBy(tag => tag.Offset).ToArray();
            for (int index = 1; index < ranges.Length; index++)
                if ((long)ranges[index - 1].Offset + ranges[index - 1].Length > ranges[index].Offset)
                    throw Error("ICC tag data partially overlap.");
        }

        internal Triple ReadWhite()
        {
            Triple white = ReadXyz(Required("wtpt"));
            if (white.X <= 0 || white.Y <= 0 || white.Z <= 0)
                throw Error("The ICC media white point must have positive XYZ components.");
            return white;
        }

        internal ProfileTransform ReadTransform(bool toPcs, IccRenderingIntent intent)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = ReadWhite();
            string prefix = toPcs ? "A2B" : "B2A";
            int number = intent == IccRenderingIntent.AbsoluteColorimetric ? 1 : (int)intent;
            if (tags.TryGetValue(Sig(prefix + number), out Tag tag) ||
                tags.TryGetValue(Sig(prefix + "0"), out tag))
            {
                if (!toPcs && IsCompatibilityDestination(tag))
                    throw Error("This built-in XYB or HDR profile has only a compatibility B2A transform and cannot be used as a conversion destination.");
                uint type = Signature(tag.Offset);
                Pipeline lut = ReadPipeline(tag, toPcs);
                return new(grayscale, lab, null, null, lut, type == Sig("mft2") && lab);
            }
            if (grayscale)
            {
                Curve curve = ReadCurve(Required("kTRC"), out _);
                if (!toPcs)
                    curve.ValidateInverse(parameter, cancellationToken);
                return new(true, lab, [curve], null, null, false);
            }
            if (lab)
                throw Error("An RGB matrix/TRC profile requires an XYZ PCS or a supported LUT transform.");
            Curve[] curves = [ReadCurve(Required("rTRC"), out _),
                ReadCurve(Required("gTRC"), out _), ReadCurve(Required("bTRC"), out _)];
            Triple r = ReadXyz(Required("rXYZ")), g = ReadXyz(Required("gXYZ")), b = ReadXyz(Required("bXYZ"));
            Matrix matrix = new([r.X, g.X, b.X, r.Y, g.Y, b.Y, r.Z, g.Z, b.Z, 0, 0, 0]);
            if (!toPcs)
            {
                matrix = matrix.Inverse(parameter);
                foreach (Curve curve in curves)
                    curve.ValidateInverse(parameter, cancellationToken);
            }
            return new(false, false, curves, matrix, null, false);
        }

        private bool IsCompatibilityDestination(Tag tag)
        {
            // Jpegli's emitted identity B2A0 does not invert its XYB/HDR A2B0.
            // Match the writer identity and structural markers, not human descriptions.
            if (Signature(80) != Sig("jxl ") || tag.Length != 80 || Signature(tag.Offset) != Sig("mBA ") ||
                bytes[tag.Offset + 8] != 3 || bytes[tag.Offset + 9] != 3 || UInt32(tag.Offset + 12) != 32)
                return false;
            for (int index = 16; index < 32; index += 4)
                if (UInt32(tag.Offset + index) != 0)
                    return false;
            for (int channel = 0; channel < 3; channel++)
            {
                int offset = tag.Offset + 32 + channel * 16;
                if (Signature(offset) != Sig("para") || UInt16(offset + 8) != 0 || Fixed(offset + 12) != 1)
                    return false;
            }
            return tags.ContainsKey(Sig("A2B0"));
        }

        private Pipeline ReadPipeline(Tag tag, bool toPcs)
        {
            uint type = Signature(tag.Offset);
            if (type != Sig("mft1") && type != Sig("mft2") &&
                type != Sig(toPcs ? "mAB " : "mBA "))
                throw Error("The selected ICC transform uses an unsupported tag type.");
            Require(tag, 0, type == Sig("mft1") ? 48 : type == Sig("mft2") ? 52 : 32);
            int inputChannels = bytes[tag.Offset + 8], outputChannels = bytes[tag.Offset + 9];
            int deviceChannels = grayscale ? 1 : 3;
            if (inputChannels != (toPcs ? deviceChannels : 3) || outputChannels != (toPcs ? 3 : deviceChannels))
                throw Error("The ICC LUT channel counts do not match the device and PCS spaces.");
            if (type == Sig("mft1") || type == Sig("mft2"))
                return ReadLegacyPipeline(tag, toPcs, type == Sig("mft2"), inputChannels, outputChannels);
            int bOffset = ElementOffset(tag, 12), matrixOffset = ElementOffset(tag, 16);
            int mOffset = ElementOffset(tag, 20), clutOffset = ElementOffset(tag, 24), aOffset = ElementOffset(tag, 28);
            if (bOffset == 0 || (matrixOffset == 0) != (mOffset == 0) ||
                (clutOffset == 0) != (aOffset == 0) || (clutOffset == 0 && inputChannels != outputChannels))
                throw Error("The ICC multi-function LUT has an invalid combination of elements.");
            int matrixChannels = toPcs ? outputChannels : inputChannels;
            if (matrixOffset != 0 && matrixChannels != 3)
                throw Error("The ICC multi-function matrix requires three channels.");
            List<Tag> elements = [];
            Curve[] b = ReadCurves(tag, bOffset, toPcs ? outputChannels : inputChannels, elements);
            Curve[]? a = aOffset == 0 ? null : ReadCurves(tag, aOffset, toPcs ? inputChannels : outputChannels, elements);
            Curve[]? m = mOffset == 0 ? null : ReadCurves(tag, mOffset, matrixChannels, elements);
            Matrix? matrix = matrixOffset == 0 ? null : ReadMatrix(tag, matrixOffset, true);
            if (matrix is not null)
                elements.Add(new(matrixOffset, 48));
            Clut? clut = clutOffset == 0 ? null : ReadClut(tag, clutOffset, inputChannels, outputChannels, elements);
            Tag[] ranges = elements.Distinct().OrderBy(element => element.Offset).ToArray();
            for (int index = 1; index < ranges.Length; index++)
                if ((long)ranges[index - 1].Offset + ranges[index - 1].Length > ranges[index].Offset)
                    throw Error("ICC multi-function elements partially overlap.");
            return new(a, b, m, matrix, clut, toPcs ? PipelineKind.AToB : PipelineKind.BToA);
        }

        private Pipeline ReadLegacyPipeline(Tag tag, bool toPcs, bool sixteenBit, int inputChannels, int outputChannels)
        {
            if (!sixteenBit && !lab)
                throw Error("ICC lut8Type has no standardized XYZ PCS encoding; use a Lab LUT or a 16-bit XYZ transform.");
            int grid = bytes[tag.Offset + 10];
            if (grid < 2)
                throw Error("An ICC CLUT needs at least two grid points per dimension.");
            int inputEntries = sixteenBit ? UInt16(tag.Offset + 48) : 256;
            int outputEntries = sixteenBit ? UInt16(tag.Offset + 50) : 256;
            if (inputEntries is < 2 or > 4096 || outputEntries is < 2 or > 4096)
                throw Error("The ICC LUT table lengths must be between two and 4096.");
            Matrix matrix = ReadMatrix(tag, 12, false);
            if (toPcs || lab)
            {
                if (!matrix.IsIdentity)
                    throw Error("An ICC lut8/lut16 matrix must be identity unless its input is PCSXYZ.");
            }
            int position = sixteenBit ? 52 : 48;
            Curve[] input = ReadTables(tag, ref position, inputChannels, inputEntries, sixteenBit ? 2 : 1);
            int[] dimensions = Enumerable.Repeat(grid, inputChannels).ToArray();
            Clut clut = ReadClutData(tag, ref position, dimensions, outputChannels, sixteenBit ? 2 : 1);
            Curve[] output = ReadTables(tag, ref position, outputChannels, outputEntries, sixteenBit ? 2 : 1);
            return new(input, output, null, matrix, clut, PipelineKind.Legacy);
        }

        private int ElementOffset(Tag tag, int field)
        {
            uint offset = UInt32(tag.Offset + field);
            if (offset == 0)
                return 0;
            if (offset < 32 || (offset & 3) != 0 || offset >= tag.Length)
                throw Error("An ICC multi-function element offset is invalid.");
            return (int)offset;
        }

        private Curve[] ReadCurves(Tag tag, int offset, int count, List<Tag> elements)
        {
            Curve[] curves = new Curve[count];
            for (int channel = 0; channel < count; channel++)
            {
                Require(tag, offset, 12);
                curves[channel] = ReadCurve(new(tag.Offset + offset, tag.Length - offset), out int length);
                elements.Add(new(offset, length));
                long next = (long)offset + ((length + 3L) & ~3L);
                if (next > tag.Length && channel != count - 1)
                    throw Error("The ICC embedded curves are truncated.");
                offset = (int)next;
            }
            return curves;
        }

        private Curve ReadCurve(Tag tag, out int length)
        {
            Require(tag, 0, 12);
            uint type = Signature(tag.Offset);
            if (type == Sig("curv"))
            {
                uint count = UInt32(tag.Offset + 8);
                if (count > (tag.Length - 12) / 2)
                    throw Error("The ICC curve table is truncated.");
                length = 12 + (int)count * 2;
                if (count == 0)
                    return Curve.Identity;
                if (count == 1)
                {
                    double gamma = UInt16(tag.Offset + 12) / 256.0;
                    if (gamma <= 0)
                        throw Error("The ICC gamma must be positive.");
                    return new(0, [gamma], parameter);
                }
                int position = 12;
                return new(ReadValues(tag, ref position, (int)count, 2));
            }
            if (type != Sig("para"))
                throw Error("The ICC curve uses an unsupported tag type.");
            int function = UInt16(tag.Offset + 8);
            int parameterCount = function switch { 0 => 1, 1 => 3, 2 => 4, 3 => 5, 4 => 7, _ => 0 };
            if (parameterCount == 0)
                throw Error("Only ICC parametric curve functions zero through four are supported.");
            length = 12 + parameterCount * 4;
            Require(tag, 0, length);
            double[] values = new double[parameterCount];
            for (int index = 0; index < values.Length; index++)
                values[index] = Fixed(tag.Offset + 12 + index * 4);
            if (values[0] <= 0 || (function != 0 && values[1] <= 0))
                throw Error("The ICC parametric curve has an undefined exponent or scale.");
            if (function >= 3)
            {
                double branchStart = Math.Max(0, values[4]);
                if (branchStart <= 1 && values[1] * branchStart + values[2] < -1e-12)
                    throw Error("The ICC parametric curve raises a negative base to a power.");
            }
            Curve curve = new(function, values, parameter);
            // Positive power scales are monotonic within each segment. Endpoints
            // and both sides of the join cover their extrema and undefined values.
            _ = curve.Apply(0);
            _ = curve.Apply(1);
            if (function >= 3 && values[4] is > 0 and < 1)
            {
                _ = curve.Apply(Math.BitDecrement(values[4]));
                _ = curve.Apply(values[4]);
            }
            return curve;
        }

        private Curve[] ReadTables(Tag tag, ref int position, int channels, int entries, int precision)
        {
            Curve[] curves = new Curve[channels];
            for (int channel = 0; channel < channels; channel++)
                curves[channel] = new(ReadValues(tag, ref position, entries, precision));
            return curves;
        }

        private double[] ReadValues(Tag tag, ref int position, int count, int precision)
        {
            Require(tag, position, (long)count * precision);
            double[] values = new double[count];
            for (int index = 0; index < count; index++)
            {
                CheckCancellation(index);
                values[index] = precision == 1 ? bytes[tag.Offset + position++] / 255.0
                    : UInt16(tag.Offset + position) / 65535.0;
                if (precision == 2)
                    position += 2;
            }
            return values;
        }

        private Matrix ReadMatrix(Tag tag, int offset, bool hasOffsets)
        {
            Require(tag, offset, hasOffsets ? 48 : 36);
            double[] values = new double[12];
            for (int index = 0; index < (hasOffsets ? 12 : 9); index++)
                values[index] = Fixed(tag.Offset + offset + index * 4);
            return new(values);
        }

        private Clut ReadClut(Tag tag, int offset, int inputs, int outputs, List<Tag> elements)
        {
            Require(tag, offset, 20);
            int[] dimensions = new int[inputs];
            for (int index = 0; index < inputs; index++)
                dimensions[index] = bytes[tag.Offset + offset + index];
            int precision = bytes[tag.Offset + offset + 16];
            if (precision is not (1 or 2))
                throw Error("The ICC CLUT must use eight-bit or sixteen-bit entries.");
            int position = offset + 20;
            Clut clut = ReadClutData(tag, ref position, dimensions, outputs, precision);
            elements.Add(new(offset, position - offset));
            return clut;
        }

        private Clut ReadClutData(Tag tag, ref int position, int[] dimensions, int outputs, int precision)
        {
            long count = outputs;
            foreach (int grid in dimensions)
            {
                if (grid < 2)
                    throw Error("An ICC CLUT needs at least two grid points per dimension.");
                count *= grid;
            }
            Require(tag, position, count * precision);
            if (count > int.MaxValue)
                throw Error("The ICC CLUT is too large.");
            return new(dimensions, outputs, ReadValues(tag, ref position, (int)count, precision));
        }

        private Triple ReadXyz(Tag tag)
        {
            Require(tag, 0, 20);
            if (Signature(tag.Offset) != Sig("XYZ "))
                throw Error("The ICC colorant or white-point tag must have XYZ type.");
            return new(Fixed(tag.Offset + 8), Fixed(tag.Offset + 12), Fixed(tag.Offset + 16));
        }

        private Tag Required(string name) => tags.TryGetValue(Sig(name), out Tag tag) ? tag
            : throw Error($"The ICC profile is missing the {name} transform tag.");

        private void Require(Tag tag, int offset, long length)
        {
            if (offset < 0 || length < 0 || offset > tag.Length || length > tag.Length - offset)
                throw Error("An ICC transform element is truncated or has an invalid length.");
        }

        private void CheckCancellation(int index)
        {
            if ((index & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
        }

        private uint Signature(int offset) => UInt32(offset);
        private uint UInt32(int offset) => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset, 4));
        private ushort UInt16(int offset) => BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));
        private double Fixed(int offset) => BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset, 4)) / 65536.0;
        private ArgumentException Error(string message) => new(message, parameter);
        private readonly record struct Tag(int Offset, int Length);
    }

    private static uint Sig(string name) => (uint)name[0] << 24 | (uint)name[1] << 16 | (uint)name[2] << 8 | name[3];

    private sealed class Curve
    {
        internal static readonly Curve Identity = new(0, [1]);
        private readonly int function;
        private readonly double[] values;
        private readonly bool table;
        private readonly string parameter;

        internal Curve(int function, double[] values, string parameter = "SourceIccProfile")
        {
            this.function = function;
            this.values = values;
            this.parameter = parameter;
        }

        internal Curve(double[] values)
        {
            this.values = values;
            table = true;
            parameter = "SourceIccProfile";
        }

        internal double Apply(double input)
        {
            double x = Clamp(input);
            if (table)
            {
                double position = x * (values.Length - 1);
                int index = Math.Min((int)position, values.Length - 2);
                return values[index] + (values[index + 1] - values[index]) * (position - index);
            }
            double result;
            if (function == 0)
                result = Math.Pow(x, values[0]);
            else if (function <= 2)
            {
                double c = function == 2 ? values[3] : 0;
                result = x >= -values[2] / values[1]
                    ? Math.Pow(Math.Max(0, values[1] * x + values[2]), values[0]) + c : c;
            }
            else
            {
                double e = function == 4 ? values[5] : 0, f = function == 4 ? values[6] : 0;
                result = x >= values[4]
                    ? Math.Pow(Math.Max(0, values[1] * x + values[2]), values[0]) + e
                    : values[3] * x + f;
            }
            if (!double.IsFinite(result))
                throw new ArgumentException("The ICC parametric curve produces a non-finite value.", parameter);
            return Clamp(result);
        }

        internal void ValidateInverse(string parameter, CancellationToken cancellationToken)
        {
            if (table)
            {
                for (int index = 1; index < values.Length; index++)
                {
                    if ((index & 1023) == 0)
                        cancellationToken.ThrowIfCancellationRequested();
                    if (values[index] < values[index - 1])
                        throw new ArgumentException("The destination ICC curve must be monotonically increasing.", parameter);
                }
            }
            else if (function >= 3)
            {
                double d = values[4];
                if ((d > 0 && values[3] < 0) || (d > 0 && d <= 1 &&
                    Apply(Math.BitDecrement(d)) > Apply(d) + 1e-12))
                    throw new ArgumentException("The destination ICC curve must be monotonically increasing.", parameter);
            }
            if (Apply(1) <= Apply(0))
                throw new ArgumentException("The destination ICC curve has no usable inverse.", parameter);
        }

        internal double Inverse(double input)
        {
            double y = Math.Clamp(input, Apply(0), Apply(1));
            if (table)
            {
                // Find the first entry >= y. Exact flats use the highest x,
                // except a terminal flat uses its lowest x (ICC Annex F.1).
                int low = 0, high = values.Length;
                while (low < high)
                {
                    int middle = low + (high - low) / 2;
                    if (values[middle] < y)
                        low = middle + 1;
                    else
                        high = middle;
                }
                int upper = low;
                if (values[upper] == y)
                {
                    if (y == values[^1])
                        return (double)upper / (values.Length - 1);
                    high = values.Length;
                    while (low < high)
                    {
                        int middle = low + (high - low) / 2;
                        if (values[middle] <= y)
                            low = middle + 1;
                        else
                            high = middle;
                    }
                    return (double)(low - 1) / (values.Length - 1);
                }
                int lower = upper - 1;
                return (lower + (y - values[lower]) / (values[upper] - values[lower])) / (values.Length - 1);
            }
            if (function == 0)
                return Math.Pow(y, 1 / values[0]);
            if (function <= 2)
            {
                double c = function == 2 ? values[3] : 0;
                return Clamp((Math.Pow(Math.Max(0, y - c), 1 / values[0]) - values[2]) / values[1]);
            }
            double d = values[4], e = function == 4 ? values[5] : 0, f = function == 4 ? values[6] : 0;
            if (d > 1)
                return Clamp((y - f) / values[3]);
            if (d > 0)
            {
                double lowerJoin = Clamp(values[3] * d + f), upperJoin = Apply(d);
                // A clipped terminal plateau may start in the linear segment and
                // continue through the join. Annex F chooses its first x.
                if (y == 1 && lowerJoin == 1)
                    return Clamp((y - f) / values[3]);
                if (y < upperJoin)
                {
                    // Fixed-point curve coefficients can produce a discontinuous
                    // join. In its gap choose the nearest attainable curve value.
                    // Preserve a lower-side choice through the float output API:
                    // the preceding double would round back to the upper branch.
                    double preceding = MathF.BitDecrement((float)d);
                    if (y >= lowerJoin)
                        return y - lowerJoin <= upperJoin - y ? preceding : d;
                    return values[3] > 0 ? Clamp(Math.Min(preceding, (y - f) / values[3])) : preceding;
                }
            }
            return Clamp(Math.Max(d, (Math.Pow(Math.Max(0, y - e), 1 / values[0]) - values[2]) / values[1]));
        }
    }

    private sealed class Matrix(double[] values)
    {
        internal bool IsIdentity => values[0] == 1 && values[4] == 1 && values[8] == 1 &&
            values[1] == 0 && values[2] == 0 && values[3] == 0 && values[5] == 0 && values[6] == 0 && values[7] == 0;

        internal Triple Apply(Triple input, bool clamp)
        {
            double x = values[0] * input.X + values[1] * input.Y + values[2] * input.Z + values[9];
            double y = values[3] * input.X + values[4] * input.Y + values[5] * input.Z + values[10];
            double z = values[6] * input.X + values[7] * input.Y + values[8] * input.Z + values[11];
            return clamp ? new(Clamp(x), Clamp(y), Clamp(z)) : new(x, y, z);
        }

        internal Matrix Inverse(string parameter)
        {
            double a = values[0], b = values[1], c = values[2], d = values[3], e = values[4], f = values[5],
                g = values[6], h = values[7], i = values[8];
            double determinant = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
            if (determinant == 0 || !double.IsFinite(determinant))
                throw new ArgumentException("The destination ICC matrix is singular.", parameter);
            double[] inverse = [(e * i - f * h) / determinant, (c * h - b * i) / determinant, (b * f - c * e) / determinant,
                (f * g - d * i) / determinant, (a * i - c * g) / determinant, (c * d - a * f) / determinant,
                (d * h - e * g) / determinant, (b * g - a * h) / determinant, (a * e - b * d) / determinant, 0, 0, 0];
            if (inverse.Any(value => !double.IsFinite(value)))
                throw new ArgumentException("The destination ICC matrix inverse is non-finite.", parameter);
            return new(inverse);
        }
    }

    private enum PipelineKind { Legacy, AToB, BToA }

    private sealed class Pipeline(Curve[]? a, Curve[] b, Curve[]? m, Matrix? matrix, Clut? clut, PipelineKind kind)
    {
        internal Triple Apply(Triple value)
        {
            if (kind == PipelineKind.Legacy)
            {
                value = matrix!.Apply(value, true);
                value = ApplyCurves(a!, value);
                return ApplyCurves(b, clut!.Apply(value));
            }
            if (kind == PipelineKind.AToB)
            {
                if (a is not null)
                    value = clut!.Apply(ApplyCurves(a, value));
                if (m is not null)
                    value = matrix!.Apply(ApplyCurves(m, value), true);
                return ApplyCurves(b, value);
            }
            value = ApplyCurves(b, value);
            if (m is not null)
                value = ApplyCurves(m, matrix!.Apply(value, true));
            return a is null ? value : ApplyCurves(a, clut!.Apply(value));
        }

        private static Triple ApplyCurves(Curve[] curves, Triple value) => curves.Length == 1
            ? new(curves[0].Apply(value.X), 0, 0)
            : new(curves[0].Apply(value.X), curves[1].Apply(value.Y), curves[2].Apply(value.Z));
    }

    private sealed class Clut
    {
        private readonly int[] dimensions;
        private readonly int outputs;
        private readonly double[] values;
        private readonly int[] cornerOffsets;

        internal Clut(int[] dimensions, int outputs, double[] values)
        {
            this.dimensions = dimensions;
            this.outputs = outputs;
            this.values = values;
            // Corner offsets depend only on the CLUT layout. Add them to the
            // lower cell's index instead of flattening every corner per pixel.
            cornerOffsets = new int[1 << dimensions.Length];
            for (int corner = 0; corner < cornerOffsets.Length; corner++)
            {
                int index = 0;
                for (int dimension = 0; dimension < dimensions.Length; dimension++)
                    index = index * dimensions[dimension] + ((corner >> dimension) & 1);
                cornerOffsets[corner] = index * outputs;
            }
        }

        internal Triple Apply(Triple input)
        {
            Span<double> fraction = stackalloc double[3];
            Span<double> components = stackalloc double[3] { input.X, input.Y, input.Z };
            int baseIndex = 0;
            for (int dimension = 0; dimension < dimensions.Length; dimension++)
            {
                double position = Clamp(components[dimension]) * (dimensions[dimension] - 1);
                int lower = Math.Min((int)position, dimensions[dimension] - 2);
                fraction[dimension] = position - lower;
                baseIndex = baseIndex * dimensions[dimension] + lower;
            }
            baseIndex *= outputs;
            Span<double> result = stackalloc double[3];
            result.Clear();
            for (int corner = 0; corner < cornerOffsets.Length; corner++)
            {
                int index = baseIndex + cornerOffsets[corner];
                double weight = 1;
                for (int dimension = 0; dimension < dimensions.Length; dimension++)
                {
                    int high = (corner >> dimension) & 1;
                    weight *= high == 0 ? 1 - fraction[dimension] : fraction[dimension];
                }
                for (int channel = 0; channel < outputs; channel++)
                    result[channel] += weight * values[index + channel];
            }
            return new(result[0], result[1], result[2]);
        }
    }
}
