using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class RenderingIntentIccProfileTests
{
    // Keep the original factory signatures and descriptions explicit: this
    // catches a factory omitted when adding the rendering-intent overloads.
    private static readonly (string Name, string Description)[] Factories =
    [
        ("CreateSrgbProfile", "sRGB"),
        ("CreateGammaSrgbProfile", "RGB_D65_SRG_Per_g0.454545"),
        ("CreateGammaDisplayP3Profile", "RGB_D65_DCI_Per_g0.454545"),
        ("CreateGammaRec2020Profile", "RGB_D65_202_Per_g0.454545"),
        ("CreateGammaGrayscaleProfile", "Gra_D65_Per_g0.454545"),
        ("CreateRec709Profile", "RGB_D65_SRG_Per_709"),
        ("CreateRec709DisplayP3Profile", "RGB_D65_DCI_Per_709"),
        ("CreateRec2020Profile", "RGB_D65_202_Per_709"),
        ("CreateSrgbRec2020Profile", "RGB_D65_202_Per_SRG"),
        ("CreateRec2100PqProfile", "RGB_D65_202_Per_PeQ"),
        ("CreateRec2100HlgProfile", "RGB_D65_202_Per_HLG"),
        ("CreateDisplayP3Profile", "DisplayP3"),
        ("CreateDciP3Profile", "RGB_DCI_DCI_Per_DCI"),
        ("CreateLinearSrgbProfile", "RGB_D65_SRG_Per_Lin"),
        ("CreateLinearDisplayP3Profile", "RGB_D65_DCI_Per_Lin"),
        ("CreateLinearRec2020Profile", "RGB_D65_202_Per_Lin"),
        ("CreateSrgbGrayscaleProfile", "Gra_D65_Per_SRG"),
        ("CreateLinearSrgbGrayscaleProfile", "Gra_D65_Per_Lin"),
        ("CreateRec709GrayscaleProfile", "Gra_D65_Per_709"),
        ("CreateDciGrayscaleProfile", "Gra_DCI_Per_DCI"),
        ("CreateRec2100PqGrayscaleProfile", "Gra_D65_Per_PeQ"),
        ("CreateRec2100HlgGrayscaleProfile", "Gra_D65_Per_HLG")
    ];

    public static TheoryData<string, string> ProfileFactories =>
        new(Factories.Select(factory => (factory.Name, factory.Description)));

    [Fact]
    public void PublicIntentValuesMatchTheIccHeaderAndEveryOriginalSignatureIsRetained()
    {
        Assert.Equal(new[] { "Perceptual", "RelativeColorimetric", "Saturation", "AbsoluteColorimetric" },
            Enum.GetNames<IccRenderingIntent>());
        Assert.Equal(new[] { 0, 1, 2, 3 }, Enum.GetValues<IccRenderingIntent>().Select(value => (int)value));
        Assert.Equal(Factories.Select(factory => factory.Name).Append("CreateXybProfile").Order(),
            typeof(JpegliIccProfiles).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(method => method.Name).Distinct().Order());
        foreach ((string name, _) in Factories)
        {
            MethodInfo original = Factory(name);
            MethodInfo explicitIntent = Factory(name, explicitIntent: true);
            Assert.Equal(typeof(byte[]), original.ReturnType);
            Assert.Equal(typeof(byte[]), explicitIntent.ReturnType);
            Assert.All(original.GetParameters(), parameter => Assert.False(parameter.IsOptional));
            Assert.All(explicitIntent.GetParameters(), parameter => Assert.False(parameter.IsOptional));
            Assert.Equal("renderingIntent", explicitIntent.GetParameters()[^1].Name);
        }
        Assert.Null(typeof(JpegliIccProfiles).GetMethod("CreateXybProfile", [typeof(IccRenderingIntent)]));
        Assert.Equal(0, Int32(JpegliIccProfiles.CreateXybProfile(), 64));
    }

    [Theory]
    [MemberData(nameof(ProfileFactories))]
    public void DefaultAndExplicitPerceptualProfilesAreIdenticalAndCallsOwnTheirStorage(
        string name, string description)
    {
        byte[] original = Create(name);
        Assert.Equal(description, Description(original));
        Assert.Equal(original, Create(name, IccRenderingIntent.Perceptual));
        foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
        {
            byte[] first = Create(name, intent), second = Create(name, intent);
            Assert.NotSame(first, second);
            Assert.NotSame(original, first);
            Assert.Equal(first, second);
            Array.Fill(first, (byte)0xdd);
            Assert.Equal(second, Create(name, intent));
        }
        Assert.Equal(original, Create(name));
    }

    [Theory]
    [MemberData(nameof(ProfileFactories))]
    public void HeaderDescriptionsAndIdsMatchTheSelectedIntentWithoutChangingTransforms(
        string name, string description)
    {
        byte[] original = Create(name);
        Dictionary<string, Tag> originalTags = Tags(original);
        foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
        {
            byte[] profile = Create(name, intent);
            Assert.Equal(profile.Length, Int32(profile, 0));
            Assert.Equal((int)intent, Int32(profile, 64));
            Assert.Equal(ExpectedDescription(name, description, intent), Description(profile));
            Assert.Equal(original[4..64], profile[4..64]);
            Assert.Equal(original[68..84], profile[68..84]);
            Assert.Equal(original[100..128], profile[100..128]);

            byte[] normalized = profile.ToArray();
            Array.Clear(normalized, 44, 4);
            Array.Clear(normalized, 64, 4);
            Array.Clear(normalized, 84, 16);
            Assert.Equal(profile[84..100], MD5.HashData(normalized));

            Dictionary<string, Tag> tags = Tags(profile);
            Assert.Equal(originalTags.Keys, tags.Keys);
            foreach ((string signature, Tag tag) in tags)
            {
                if (signature == "desc")
                    continue;
                Tag reference = originalTags[signature];
                Assert.Equal(original[reference.Offset..(reference.Offset + reference.Length)],
                    profile[tag.Offset..(tag.Offset + tag.Length)]);
            }
        }
    }

    [Theory]
    [MemberData(nameof(ProfileFactories))]
    public void UndefinedIntentsAreRejectedWithThePublicParameterName(string name, string description)
    {
        Assert.NotEmpty(description);
        foreach (int value in new[] { -1, 4, 256, int.MinValue, int.MaxValue })
        {
            TargetInvocationException wrapper = Assert.Throws<TargetInvocationException>(() =>
                Create(name, (IccRenderingIntent)value));
            ArgumentOutOfRangeException error = Assert.IsType<ArgumentOutOfRangeException>(wrapper.InnerException);
            Assert.Equal("renderingIntent", error.ParamName);
        }
    }

    [Theory]
    [InlineData("CreateGammaSrgbProfile")]
    [InlineData("CreateGammaDisplayP3Profile")]
    [InlineData("CreateGammaRec2020Profile")]
    [InlineData("CreateGammaGrayscaleProfile")]
    public void ExplicitIntentOverloadsRetainGammaValidation(string name)
    {
        foreach (double gamma in new[] { double.NaN, double.NegativeInfinity, double.PositiveInfinity, 0.999, 8192.001 })
            foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
            {
                TargetInvocationException wrapper = Assert.Throws<TargetInvocationException>(() =>
                    Factory(name, explicitIntent: true).Invoke(null, [gamma, intent]));
                ArgumentOutOfRangeException error = Assert.IsType<ArgumentOutOfRangeException>(wrapper.InnerException);
                Assert.Equal("gamma", error.ParamName);
            }
        foreach (double gamma in new[] { 1.0, 8192.0 })
            foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
                Assert.NotNull(Factory(name, explicitIntent: true).Invoke(null, [gamma, intent]));
    }

    [Theory]
    [InlineData("CreateSrgbProfile", 0, JpegColorSpace.YCbCr, false)]
    [InlineData("CreateGammaDisplayP3Profile", 1, JpegColorSpace.Rgb, true)]
    [InlineData("CreateSrgbGrayscaleProfile", 0, JpegColorSpace.Grayscale, true)]
    [InlineData("CreateRec2100PqGrayscaleProfile", 2, JpegColorSpace.Grayscale, false)]
    [InlineData("CreateRec2100PqProfile", 1, JpegColorSpace.YCbCr, true)]
    [InlineData("CreateRec2100HlgProfile", 2, JpegColorSpace.Rgb, false)]
    public void IntentProfilesEmbedUnchangedAndPreserveCodingAcrossPrecisionsScansAndNonseekStreams(
        string name, int kind, JpegColorSpace colorSpace, bool progressive)
    {
        const int width = 19, height = 17;
        PixelComponentLayout layout = colorSpace == JpegColorSpace.Grayscale
            ? PixelComponentLayout.Gray : PixelComponentLayout.Rgba;
        int components = layout == PixelComponentLayout.Gray ? 1 : 4;
        int stride = width * components + 5;
        Array pixels = Source(kind, width, height, stride, components);
        Array originalPixels = (Array)pixels.Clone();
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, IccProfile = Create(name), Distance = 1.375f,
            Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
            OptimizeHuffmanTables = progressive, RestartInterval = 2
        };
        byte[] expectedCoding = ReadIccAndCoding(Encode(pixels, width, height, stride, layout, options)).Coding;
        foreach (IccRenderingIntent intent in Enum.GetValues<IccRenderingIntent>())
        {
            byte[] profile = Create(name, intent);
            byte[] backing = Enumerable.Repeat((byte)0x5b, profile.Length + 11).ToArray();
            profile.CopyTo(backing, 3);
            byte[] originalBacking = backing.ToArray();
            JpegliEncoderOptions selected = options with { IccProfile = backing.AsMemory(3, profile.Length) };
            byte[] jpeg = Encode(pixels, width, height, stride, layout, selected);
            (byte[] embedded, byte[] coding) = ReadIccAndCoding(jpeg);
            Assert.Equal(profile, embedded);
            Assert.Equal(expectedCoding, coding);
            JpegDocument document = JpegDocument.Read(jpeg, progressive, 2, selected.ProgressiveRefinementBits);
            Assert.Equal((width, height), (document.Width, document.Height));
            Assert.Equal(colorSpace == JpegColorSpace.Grayscale ? 1 : 3, document.Components.Count);
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            Encode(output, pixels, width, height, stride, layout, selected);
            Assert.Equal(jpeg, destination.ToArray());
            Assert.False(output.Closed);
            Assert.Equal(originalBacking, backing);
        }
        Assert.Equal(originalPixels.Cast<object>(), pixels.Cast<object>());
    }

    // Independent complete-profile fixtures from unchanged google/jpegli
    // lib/cms/jpegli_cms_internal.h at 031a0077f5799a6041004267fc12b956c1f52a20.
    // MSVC ARM64 C++17 /O2 /fp:strict; gamma factories use exponent 2.2;
    // HDR uses the pinned builder's default 3D tone mapping. Every RGB and
    // grayscale factory is covered for all four native rendering intents.
    [Theory]
    [InlineData("CreateSrgbProfile", 0, 504, "sRGB", "1406EFC5429250FEBE778C182EB5E5168AA14B2CAEFCF772211A2EC2ECF31659", "A3379298068DF11976289D87A33230AA")]
    [InlineData("CreateSrgbProfile", 1, 536, "RGB_D65_SRG_Rel_SRG", "D7F9E32AC2A06A7E3865A828E865DAFB7CF1947584D3C06EC97A660C59786B51", "425B410C0A8285639EB234EF7C6DA647")]
    [InlineData("CreateSrgbProfile", 2, 536, "RGB_D65_SRG_Sat_SRG", "CA2731A3499AD3130D0F4EB29BC7C7AC12223CF20C5105D20A6C957865E4E6B4", "FA50180B9BAF603FB4E616219CC52198")]
    [InlineData("CreateSrgbProfile", 3, 536, "RGB_D65_SRG_Abs_SRG", "6848B5773C2E9F90D8AA2A62F517F79C9E7953E106EDB01C9B0B58B5257A6389", "0C50DEFBC368D066B1C8B421BA571BF0")]
    [InlineData("CreateDisplayP3Profile", 0, 516, "DisplayP3", "A35258ED146F3F0A6A24C25B102AAF2A02C9721CBB647422FA5A3F1CFD132C96", "DF9F5B76A9627A080C7EDDA1399115DC")]
    [InlineData("CreateDisplayP3Profile", 1, 536, "RGB_D65_DCI_Rel_SRG", "128399778F07C073EA45C68C7962128DE6583DC47A4500AE8F5B636D3EDECD6D", "FDDDAD1AE16B2FD710915B7A1AAF3630")]
    [InlineData("CreateDisplayP3Profile", 2, 536, "RGB_D65_DCI_Sat_SRG", "5C799686D43633C124F3FE9CE7ECC57ECF910B4CF008DC9F64473D5647D78201", "B11A0F745A979D824814E2689CE50489")]
    [InlineData("CreateDisplayP3Profile", 3, 536, "RGB_D65_DCI_Abs_SRG", "A44781DF1AE1EEAAEB4F70280C653D81DDA7638157650C16B38357EB6C6E3CD7", "767931510EB3D26883961C91EB931E8B")]
    [InlineData("CreateSrgbRec2020Profile", 0, 536, "RGB_D65_202_Per_SRG", "DD8321748965BBCFBF7868C7FA478079C091EB38D7ABD3696319992434E21F23", "F2E724B7691C1EBB156A779F3F845AFF")]
    [InlineData("CreateSrgbRec2020Profile", 1, 536, "RGB_D65_202_Rel_SRG", "7B905EF6C193DA187D1D7AE805336BCCC43B2893632C9BDDC36F5CAD39FFAFB8", "33CE3AB3FACE7865C059ADED09B6BAA8")]
    [InlineData("CreateSrgbRec2020Profile", 2, 536, "RGB_D65_202_Sat_SRG", "25383923BA08D36533C2B8D9803A549725CC762A583D1E4B3075BB181F6FA80A", "4FD233911E13E05B6689F04CE7489825")]
    [InlineData("CreateSrgbRec2020Profile", 3, 536, "RGB_D65_202_Abs_SRG", "F51CDCA109CAE950BC1EEE2654157A2500143D0171AE1F105A3C2194FC53B1AF", "BCA46A1576751CC30E77863800E71A2C")]
    [InlineData("CreateLinearSrgbProfile", 0, 536, "RGB_D65_SRG_Per_Lin", "138747C4D990CAFE065BB407D55219D3B7FB7F22CF803FCD07D85915514028D7", "AC25B029F881260EB2154A2D8B1CAC5B")]
    [InlineData("CreateLinearSrgbProfile", 1, 536, "RGB_D65_SRG_Rel_Lin", "68155F2BD3F362CA3A62BDD1620BF3A7BFCA7C0554062BB2A5FB5DFDDF0D73F4", "8EA3E5F0307CF8C739BCC35EF6A24336")]
    [InlineData("CreateLinearSrgbProfile", 2, 536, "RGB_D65_SRG_Sat_Lin", "D303497AD2C178EF88769C405681C4DA10FCBF5867FE02FE6EC5742F0148F5F7", "72F3DAD14D99F38EDCE0D1F53A428B69")]
    [InlineData("CreateLinearSrgbProfile", 3, 536, "RGB_D65_SRG_Abs_Lin", "399EA7DAFFB9F0200F75A3FED4187DCF3C6F8B40027D5F7BBF0320F9155AFB63", "F54F7F6D457B7141276728FB23A59FBA")]
    [InlineData("CreateLinearDisplayP3Profile", 0, 536, "RGB_D65_DCI_Per_Lin", "49201BF4652A43DFE19017C07E2FAF75EA2D43BD710F90195F6164E5E0C69A15", "C52340FED739BD5472CF28CAA75A250C")]
    [InlineData("CreateLinearDisplayP3Profile", 1, 536, "RGB_D65_DCI_Rel_Lin", "54FB70EE405AEC537F21E8C4D92AF975AEA399A3F98E59EB6AA74554CE53A35A", "55C217930B65C3C814472A91FA2503D1")]
    [InlineData("CreateLinearDisplayP3Profile", 2, 536, "RGB_D65_DCI_Sat_Lin", "936AEFB5CBE71B0EC4BD0BA9CACB373B680C93FF0D67897C20AA9C4BD7EA57C0", "AA26E5FDA07B859E65F3B527B7DF3382")]
    [InlineData("CreateLinearDisplayP3Profile", 3, 536, "RGB_D65_DCI_Abs_Lin", "2E8BCA8E9D6BD2C8A63603A51244AF7BBEFAEE5C717FAB63EE89D39EB0EE3E77", "413F51234FF477BA33AD398401F66B98")]
    [InlineData("CreateLinearRec2020Profile", 0, 536, "RGB_D65_202_Per_Lin", "E403CC09CAF20E276AC86B96B1D0A009B7B6ACFBC654775F311D2BCEDCB984D9", "899A91EB98AABBA5AF5036683720388C")]
    [InlineData("CreateLinearRec2020Profile", 1, 536, "RGB_D65_202_Rel_Lin", "8210903E196C88BFC368C602D1B413D7CAF5339AA4FF58E25BEB7A4F50E37CA8", "BB7E83AECA02D7DEFAC82890C44622DC")]
    [InlineData("CreateLinearRec2020Profile", 2, 536, "RGB_D65_202_Sat_Lin", "24B913DC471C0BDB23EE12B88488C51BBA81DCEF8402583336EB01C72B821A20", "677278A0DC24DA0B94C9CE7C5A63B4CB")]
    [InlineData("CreateLinearRec2020Profile", 3, 536, "RGB_D65_202_Abs_Lin", "D3C89C804692B16D420AF1689349C974C941EAA8D7F0B9E861E1E757DF7F90DA", "0763CA1BBDB7532F407034FF742BBDE4")]
    [InlineData("CreateRec709Profile", 0, 536, "RGB_D65_SRG_Per_709", "045567821C2B986EA4218C62D67068C26415CB5C7655FE7AE0F6C9CE0F5F6DAF", "CFBA11E85E5933B6BA8D908E88DFFB58")]
    [InlineData("CreateRec709Profile", 1, 536, "RGB_D65_SRG_Rel_709", "F708D24C35DED631D1A400FA9EE2ED2641661791162E3D4568380719A53162AF", "B067A93E125ACB3772B3F56933649780")]
    [InlineData("CreateRec709Profile", 2, 536, "RGB_D65_SRG_Sat_709", "250DBE984C5E2E4B6901104164D55F5FCED3C3CB6A4F836EA0313EFFE4E0714C", "A0D549605F5C2FF746E9F6BCED1F05A4")]
    [InlineData("CreateRec709Profile", 3, 536, "RGB_D65_SRG_Abs_709", "8332E8D70665A59F604993A9A92EE5CE96A939EC4F39295FBCCED5BD3DBDC869", "3900160773DB40220E6A414263CBB29F")]
    [InlineData("CreateRec709DisplayP3Profile", 0, 536, "RGB_D65_DCI_Per_709", "277501682AE976513DA5E633BC98612B99D4B467E00070E29E5DE94F4A2ACD6E", "D6EE73D61B2F87577400D00BD925EA65")]
    [InlineData("CreateRec709DisplayP3Profile", 1, 536, "RGB_D65_DCI_Rel_709", "6520C17F58DC78186CA13C84F6595108A60CEB776C4ADD80BD0A08B77314B822", "F07653C8ED5704F0E01565D4D4F07309")]
    [InlineData("CreateRec709DisplayP3Profile", 2, 536, "RGB_D65_DCI_Sat_709", "F880B561F9F0035CD25AB6EAA5573B24B1DECEA2B20ED01A67DED8DE520947CF", "66AE784284DE248E4396E50D60A33FA0")]
    [InlineData("CreateRec709DisplayP3Profile", 3, 536, "RGB_D65_DCI_Abs_709", "3F6406315893DE67E03122762A467B19ECC704617F5C59247AA8D346A654BF4F", "5A19123031DC26654DC0552F86E8FEB8")]
    [InlineData("CreateRec2020Profile", 0, 536, "RGB_D65_202_Per_709", "B69A102609B438F3D4CE0EEE90C85E77DEAA289257599EB81FB6CCA96EF43DA7", "5E074A7982BF282A685AF94289F51A07")]
    [InlineData("CreateRec2020Profile", 1, 536, "RGB_D65_202_Rel_709", "877C5B36808954F322AF187B56C3B3EBFD1CC8BAD569F27F67F1A0C450D11B9C", "06E12BFEE4A6D89B65B6C030517DB75D")]
    [InlineData("CreateRec2020Profile", 2, 536, "RGB_D65_202_Sat_709", "031BF590C83D7B9E170A0480630127569719F6E3FB54529F362AC17DF7A4297F", "B98117A4097B3FE22CDB523AA06B7398")]
    [InlineData("CreateRec2020Profile", 3, 536, "RGB_D65_202_Abs_709", "5B7B8F622C2128E807CFF5F4498F9B4102BCD113F80CBFB96B65711DD51C523F", "A99D14C6B26335A1995C72CEE242DA26")]
    [InlineData("CreateDciP3Profile", 0, 536, "RGB_DCI_DCI_Per_DCI", "D39C3009847419929E2746189C3EBC10B6DF93A7519A83C78D64A26778D94578", "CFF4D77412057A38BA87E9CC1ED826FD")]
    [InlineData("CreateDciP3Profile", 1, 536, "RGB_DCI_DCI_Rel_DCI", "53413E194F813165BA5FA7714360BF81E39C59EDE6080CF8B49CDB833A22B9D6", "A67CFF6759145C4C77C6F117C351D3ED")]
    [InlineData("CreateDciP3Profile", 2, 536, "RGB_DCI_DCI_Sat_DCI", "82C3FE168D7C1DF54BCF5E056F25AAFB4F30301C05A881E4EB045A3985EA670D", "CB1DC19A9ED6B718693C4A4FE6847A16")]
    [InlineData("CreateDciP3Profile", 3, 536, "RGB_DCI_DCI_Abs_DCI", "DA1BED85D6D48904633F2FE916ACE1D7A59565FDB2946C888BDCBF253A263505", "06D292C79637270B32C9A6B3165E071E")]
    [InlineData("CreateRec2100PqProfile", 0, 4344, "RGB_D65_202_Per_PeQ", "B2F97E195A8C64DD55EFF71FC1DF480A12E59D71105D7D9DABFCC23F9252C48E", "22BC30F3E3B03923D34FE5CF368CCFA6")]
    [InlineData("CreateRec2100PqProfile", 1, 4324, "Rec2100PQ", "745265D126B3BBFDB6A0E481936D5E51752D4BFED11387B83536BCBA9CE1884A", "5AD24C1614A4EABC83D43904A8600E8F")]
    [InlineData("CreateRec2100PqProfile", 2, 4344, "RGB_D65_202_Sat_PeQ", "19258CF5A23226CCDF4A2FC7570D140109A2474237FA8699427A0F6DB77BDB18", "EACCD1C5137E163C5E87B73322E3BE52")]
    [InlineData("CreateRec2100PqProfile", 3, 4344, "RGB_D65_202_Abs_PeQ", "63E020914913198E14CB5F551B8E0C7F55C05AEE6CB14F77C9E98F343E43A0DD", "BC638EC8A4DB1CFD16B099BEB167CE35")]
    [InlineData("CreateRec2100HlgProfile", 0, 4344, "RGB_D65_202_Per_HLG", "CB3A3012517E21922D895462F66841F6EDEDDE8031CC3CD3041A12AF8536A556", "D2BA1D3F609E15C6612C1CDE2CC98D15")]
    [InlineData("CreateRec2100HlgProfile", 1, 4324, "Rec2100HLG", "8C50CF64E267FD7DE589200757166A63CB8445E857FE7796DA727D41BD8DCD41", "B252BC24B541E49061B981815D91AA01")]
    [InlineData("CreateRec2100HlgProfile", 2, 4344, "RGB_D65_202_Sat_HLG", "5DD75F6056508FC31AD97840FE140E102F5DC7C0018003FA71D05437A4E18370", "749767A7DFB4104198C5E21AF8D20BDB")]
    [InlineData("CreateRec2100HlgProfile", 3, 4344, "RGB_D65_202_Abs_HLG", "30C52909B67BFE7994FE022F54550B5BDE5DA3D73ABE5AFDA65479459C7CB5D3", "2E1554B0CB15F545C500C4E8F58CB7FB")]
    [InlineData("CreateGammaSrgbProfile", 0, 508, "RGB_D65_SRG_Per_g0.454545", "AC2E845DC09136B1E0BA9BDA284CB6A0A88977BA939666D2DDA1A0A965632325", "2C27024414F660F61D61B369ED9E7E1E")]
    [InlineData("CreateGammaSrgbProfile", 1, 508, "RGB_D65_SRG_Rel_g0.454545", "8FD66285480C22C54EFAC8EC97544B8DA95ECA4A6E5FB6BA7657444632B1768F", "C49B0B327FDF7103AF69E3EB182A2ABA")]
    [InlineData("CreateGammaSrgbProfile", 2, 508, "RGB_D65_SRG_Sat_g0.454545", "5764625ABCBC884622373F2A657ED32D7141C13D9852BE2E43D46C70244DFFFF", "C8A8E6F7D123A6A2CC4EE9C94D9D794F")]
    [InlineData("CreateGammaSrgbProfile", 3, 508, "RGB_D65_SRG_Abs_g0.454545", "455457052091EAC8454C082C37954BE937559069008CD9086D8DC312DD432A49", "644263D28A56D9717A4C092CF6F1F3D2")]
    [InlineData("CreateGammaDisplayP3Profile", 0, 508, "RGB_D65_DCI_Per_g0.454545", "91229100CD4881E0B532F59C4903366EF0319B22A2186BC5F3A0A6FFBB420ABE", "1C788F9A5E5A76A22F2975B38FDC9D7E")]
    [InlineData("CreateGammaDisplayP3Profile", 1, 508, "RGB_D65_DCI_Rel_g0.454545", "E56380004FEC813ED0BD3CDBECA9B33F120B33DA2A4D1A14899B7F5BE8BECBC5", "50948FA52122172E8DDECB48E2179372")]
    [InlineData("CreateGammaDisplayP3Profile", 2, 508, "RGB_D65_DCI_Sat_g0.454545", "28B4EA5A5D831C08FD07F61CFD58160F4A2B089FFA3C1F19D1613D2ECC745A60", "C832EB220A90CBB90DB792C89E7754EB")]
    [InlineData("CreateGammaDisplayP3Profile", 3, 508, "RGB_D65_DCI_Abs_g0.454545", "8ECEFE6041B07C2455DED172A2F76FAC054BEA81E97B5B4C1C846808C944D1F8", "913FD225ED3C7ABCB414601A8E471311")]
    [InlineData("CreateGammaRec2020Profile", 0, 508, "RGB_D65_202_Per_g0.454545", "C8CBFD51C18758493286F90838EAA5EBE376C81F0C4AC6143767F91881636CC2", "15AE98FBE0D14BACEF0F4C6CF3840102")]
    [InlineData("CreateGammaRec2020Profile", 1, 508, "RGB_D65_202_Rel_g0.454545", "988C2BC0C86BEBCFB60927CC0EB62265BC7661E6A1CCD909A01F1971971B3C63", "D784C40258FAA533352CBA935C9C423E")]
    [InlineData("CreateGammaRec2020Profile", 2, 508, "RGB_D65_202_Sat_g0.454545", "D96630C0AFA23226D72C4A7BF16529B18C5BB63FE33B1BA660604776308DAB6C", "B616A03F909EEDF0DD68A88E4D438532")]
    [InlineData("CreateGammaRec2020Profile", 3, 508, "RGB_D65_202_Abs_g0.454545", "6793761ED0324987893C923B1DC7D7B6B3A5CC3AE417172B8B3B301EBFE6494D", "470F61AC297F00499E551A422849CACB")]
    [InlineData("CreateSrgbGrayscaleProfile", 0, 328, "Gra_D65_Per_SRG", "BD99A693D2BA41BA6C0E818B2D7EE2B2A313289AD5B9C66FEFD54D09FFCA6406", "57A702AE8C0F59013AAA9E549C8C5CDA")]
    [InlineData("CreateSrgbGrayscaleProfile", 1, 328, "Gra_D65_Rel_SRG", "48D006762D583F6E354A3223C0A5AEAFF7F45A324E229D237D69630BCC170241", "299AB5112C852FC36DE6088516DB10CD")]
    [InlineData("CreateSrgbGrayscaleProfile", 2, 328, "Gra_D65_Sat_SRG", "A740CEC0E4F43FBA3762371FEDDD1EC850C29273CA7CB5008055A46E17CBA49C", "7242AE4265A4745546C264F22A676FD4")]
    [InlineData("CreateSrgbGrayscaleProfile", 3, 328, "Gra_D65_Abs_SRG", "1859F0F5516436B7B58425C56D9F3A0803F00793157783B6EC7CBD5836DF1DED", "81859C899F65AE311171E765EC794CC0")]
    [InlineData("CreateLinearSrgbGrayscaleProfile", 0, 328, "Gra_D65_Per_Lin", "D05AF72D689C7497399BFE7BEB87AE7C3687AAB4AB9347C16DCAAFA2325C0404", "83DF0E7B027D00AC32774D1BB8D17F1D")]
    [InlineData("CreateLinearSrgbGrayscaleProfile", 1, 328, "Gra_D65_Rel_Lin", "57363D9EC00043FE6E3F40B1C3E0CC4676773011FD0818710FB26545002AC27D", "1948C0D0B76231404F39210BE0ECA278")]
    [InlineData("CreateLinearSrgbGrayscaleProfile", 2, 328, "Gra_D65_Sat_Lin", "F9685F0624CFA55E8F82414DD0517FE422FE3BBC84CD1A495E811E84EB81D1AB", "FD3BA7D2DC0CDCEE7B2451481D717ED6")]
    [InlineData("CreateLinearSrgbGrayscaleProfile", 3, 328, "Gra_D65_Abs_Lin", "C7FA974EF57AB2C0A65E1D4BAFFCCAF561EE51CDD330D0C4D3477E9102F8B3BD", "DDA520E614E743A182392A741F5D209B")]
    [InlineData("CreateRec709GrayscaleProfile", 0, 328, "Gra_D65_Per_709", "2955B00FF8629C6C518509DC1F31279828D00508B096F4EB9565D271849CBB81", "40AB32E3684939CF0CB29EB2C82798F5")]
    [InlineData("CreateRec709GrayscaleProfile", 1, 328, "Gra_D65_Rel_709", "21BB7D309296C4A133DBC959A823C6D5B6A0ABEE8F8BCEC755B478340899B6BC", "63987949042F061A2CFC614A7BA28C0E")]
    [InlineData("CreateRec709GrayscaleProfile", 2, 328, "Gra_D65_Sat_709", "1B98C059FB18E1BEDF098B296D149E743FC47D406708623FBEF2C1341E3B1679", "35E56C2E71C32703342363AB803B09A1")]
    [InlineData("CreateRec709GrayscaleProfile", 3, 328, "Gra_D65_Abs_709", "E0AEBE49BF9C414FAC99D63D4CFC88A003F78BE216A8E79E1FCAC38139AB062A", "640C47F9A27B0C3E4E7408C8EF7A7D26")]
    [InlineData("CreateDciGrayscaleProfile", 0, 328, "Gra_DCI_Per_DCI", "E3D49DE1DEE11786DBA14DE3F273B1CBBC86FD5501F3DC42CD393F737ACE82FB", "C20C5B4FA3DD24109E6DA3D52288B148")]
    [InlineData("CreateDciGrayscaleProfile", 1, 328, "Gra_DCI_Rel_DCI", "D970DAA032BCF85DE92BB5630608B4026525DAB1998AC4D1D9307C4F9FF51A26", "095D1221B485EE866B0281D711A65AB6")]
    [InlineData("CreateDciGrayscaleProfile", 2, 328, "Gra_DCI_Sat_DCI", "ABEF3264B4BD5AAD69F89C5A541D38F1E56C18ED1ED754286EEE2A7BB30EDB49", "04FAB36B81635E85CB60A6D7E5CE523B")]
    [InlineData("CreateDciGrayscaleProfile", 3, 328, "Gra_DCI_Abs_DCI", "6A2F45DB780DD6A996BCE2825C5D40E3D9F17C783EBA50146B9DCF35802AF454", "3A3B4512F7B234CF634BAF3D7D4C1137")]
    [InlineData("CreateRec2100PqGrayscaleProfile", 0, 436, "Gra_D65_Per_PeQ", "1F25816054A452DD533EF8F56203506387A9521474225B59DEB3D8D91F51BF58", "5713614D083B44B7CEA46698FE2DAF5F")]
    [InlineData("CreateRec2100PqGrayscaleProfile", 1, 436, "Gra_D65_Rel_PeQ", "EBF0E2F4AF24AD9886D35FFEA5687432BA56CA230D630335DF41F2B2606CD2B5", "31CCE3447985195058E1B75C929E6BAF")]
    [InlineData("CreateRec2100PqGrayscaleProfile", 2, 436, "Gra_D65_Sat_PeQ", "F1D0FB3E66D46BEC57AEF10B528134345CCDD0D229FE7E5540E2BB9B750DE903", "DDFA106B4724EDD077A4274B78E3FFE5")]
    [InlineData("CreateRec2100PqGrayscaleProfile", 3, 436, "Gra_D65_Abs_PeQ", "E1619972BE205B2B57F5784549CD16247FD6ADBAEBB1975C74D6ABD35A7379C7", "3B21F2AF9ABE9BC829E2F5EA08081C66")]
    [InlineData("CreateRec2100HlgGrayscaleProfile", 0, 436, "Gra_D65_Per_HLG", "0EA7002A458B45560E4FA1B9FA8B99115C2CFB5468DCA8FE494480E6D093E013", "6B40A764E4E988F46E9F399DE140CA8C")]
    [InlineData("CreateRec2100HlgGrayscaleProfile", 1, 436, "Gra_D65_Rel_HLG", "E19D615BAD5F967D4ECDA1168CD8D2973CA66C7CF793775E0CD39B9F8B462E52", "ED355BB4E23679124AD235F4C62D0CE8")]
    [InlineData("CreateRec2100HlgGrayscaleProfile", 2, 436, "Gra_D65_Sat_HLG", "F693115C894D2AF52365BD0F5F4869F983A4827E938F127DC6BBE4B5941C1459", "BC9057232EF0CAA9F0127B5DCF0B534F")]
    [InlineData("CreateRec2100HlgGrayscaleProfile", 3, 436, "Gra_D65_Abs_HLG", "AB372C25531C15A58F5915D2F41B0548303ED999BEAAB6BB8A70A364A9741E20", "68BC6A510C3262DA214E7B5AB2B63C99")]
    [InlineData("CreateGammaGrayscaleProfile", 0, 324, "Gra_D65_Per_g0.454545", "3B9ABDA49BB8C3869C7D4B09595F8BD1AD1B95C8E2F956D4EFD4FACCA808381E", "165030E6E575D21BBB727F0344B1A59B")]
    [InlineData("CreateGammaGrayscaleProfile", 1, 324, "Gra_D65_Rel_g0.454545", "CF8F4950A44C20C14C20F80DE4C4EB9A0AE65221738B3E8C4A9ED7C7C49026EE", "FF46DC08572C9E71FBE1657CE8963825")]
    [InlineData("CreateGammaGrayscaleProfile", 2, 324, "Gra_D65_Sat_g0.454545", "4E9E94A6C1B04742D3EF3AC80820E30677E7C61B79BAB33B615887E9F9550A1E", "E5D26E286423C44A5D3598E0D75D2807")]
    [InlineData("CreateGammaGrayscaleProfile", 3, 324, "Gra_D65_Abs_g0.454545", "9439C3A802BDC258C474791C3470B27AADDE091096DFF91FD6B88899DF05DE64", "A8D686BDC13929E3FD59AAAD1B956805")]
    public void CompleteProfilesMatchThePinnedNativeFixtures(
        string name, int intent, int size, string description, string sha256, string id)
    {
        byte[] profile = Create(name, (IccRenderingIntent)intent);
        Assert.Equal(size, profile.Length);
        Assert.Equal(intent, Int32(profile, 64));
        Assert.Equal(description, Description(profile));
        Assert.Equal(sha256, Convert.ToHexString(SHA256.HashData(profile)));
        Assert.Equal(id, Convert.ToHexString(profile.AsSpan(84, 16)));
    }
    private static MethodInfo Factory(string name, bool explicitIntent = false)
    {
        bool gamma = name.StartsWith("CreateGamma", StringComparison.Ordinal);
        Type[] parameters = gamma ? [typeof(double)] : [];
        if (explicitIntent)
            parameters = [.. parameters, typeof(IccRenderingIntent)];
        MethodInfo? method = typeof(JpegliIccProfiles).GetMethod(name, parameters);
        Assert.NotNull(method);
        return method;
    }

    private static byte[] Create(string name, IccRenderingIntent? intent = null)
    {
        object[] parameters = name.StartsWith("CreateGamma", StringComparison.Ordinal) ? [2.2] : [];
        if (intent is IccRenderingIntent selected)
            parameters = [.. parameters, selected];
        return Assert.IsType<byte[]>(Factory(name, intent is not null).Invoke(null, parameters));
    }

    private static string ExpectedDescription(string name, string original, IccRenderingIntent intent)
    {
        if (intent == IccRenderingIntent.Perceptual)
            return original;
        string token = intent switch
        {
            IccRenderingIntent.RelativeColorimetric => "Rel",
            IccRenderingIntent.Saturation => "Sat",
            IccRenderingIntent.AbsoluteColorimetric => "Abs",
            _ => throw new ArgumentOutOfRangeException(nameof(intent))
        };
        if (name is "CreateRec2100PqProfile" or "CreateRec2100HlgProfile" && intent == IccRenderingIntent.RelativeColorimetric)
            return name == "CreateRec2100PqProfile" ? "Rec2100PQ" : "Rec2100HLG";
        return original switch
        {
            "sRGB" => $"RGB_D65_SRG_{token}_SRG",
            "DisplayP3" => $"RGB_D65_DCI_{token}_SRG",
            _ => original.Replace("_Per_", $"_{token}_", StringComparison.Ordinal)
        };
    }

    private static Dictionary<string, Tag> Tags(byte[] profile)
    {
        int count = Int32(profile, 128), end = 132 + count * 12;
        var tags = new Dictionary<string, Tag>();
        for (int index = 0; index < count; index++)
        {
            int entry = 132 + index * 12;
            var tag = new Tag(Int32(profile, entry + 4), Int32(profile, entry + 8));
            Assert.Equal(0, tag.Offset % 4);
            Assert.InRange(tag.Offset, end, profile.Length - 8);
            Assert.InRange(tag.Length, 8, profile.Length - tag.Offset);
            Assert.Equal(0, Int32(profile, tag.Offset + 4));
            Assert.True(tags.TryAdd(Encoding.ASCII.GetString(profile, entry, 4), tag));
        }
        Tag[] ordered = tags.Values.Distinct().OrderBy(tag => tag.Offset).ToArray();
        Assert.Equal(end, ordered[0].Offset);
        for (int index = 1; index < ordered.Length; index++)
        {
            int previousEnd = ordered[index - 1].Offset + ordered[index - 1].Length;
            Assert.Equal((previousEnd + 3) & ~3, ordered[index].Offset);
            Assert.All(profile[previousEnd..ordered[index].Offset], value => Assert.Equal(0, value));
        }
        Assert.Equal(profile.Length, ordered[^1].Offset + ordered[^1].Length);
        return tags;
    }

    private static string Description(byte[] profile)
    {
        Tag tag = Tags(profile)["desc"];
        Assert.Equal("mluc", Encoding.ASCII.GetString(profile, tag.Offset, 4));
        Assert.Equal(1, Int32(profile, tag.Offset + 8));
        Assert.Equal(12, Int32(profile, tag.Offset + 12));
        Assert.Equal("enUS", Encoding.ASCII.GetString(profile, tag.Offset + 16, 4));
        int length = Int32(profile, tag.Offset + 20), offset = Int32(profile, tag.Offset + 24);
        Assert.Equal(28, offset);
        Assert.Equal(0, length % 2);
        Assert.InRange(length, 0, tag.Length - offset);
        Assert.All(profile[(tag.Offset + offset + length)..(tag.Offset + tag.Length)], value => Assert.Equal(0, value));
        return Encoding.BigEndianUnicode.GetString(profile, tag.Offset + offset, length);
    }

    private static (byte[] Icc, byte[] Coding) ReadIccAndCoding(byte[] jpeg)
    {
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        using var coding = new MemoryStream();
        coding.Write(jpeg.AsSpan(0, 2));
        byte[]? profile = null;
        int offset = 2;
        while (jpeg[offset + 1] != 0xda)
        {
            Assert.Equal(0xff, jpeg[offset]);
            int length = BinaryPrimitives.ReadUInt16BigEndian(jpeg.AsSpan(offset + 2, 2));
            Assert.InRange(length, 2, jpeg.Length - offset - 2);
            int end = offset + 2 + length;
            if (jpeg[offset + 1] == 0xe2)
            {
                Assert.Null(profile);
                Assert.True(jpeg.AsSpan(offset + 4, 12).SequenceEqual("ICC_PROFILE\0"u8));
                Assert.Equal(1, jpeg[offset + 16]);
                Assert.Equal(1, jpeg[offset + 17]);
                profile = jpeg[(offset + 18)..end];
            }
            else
                coding.Write(jpeg.AsSpan(offset, end - offset));
            offset = end;
        }
        coding.Write(jpeg.AsSpan(offset));
        Assert.NotNull(profile);
        return (profile, coding.ToArray());
    }

    private static Array Source(int kind, int width, int height, int stride, int components)
    {
        byte[] bytes = Enumerable.Repeat((byte)0xad, (height - 1) * stride + width * components).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < components; channel++)
                    bytes[y * stride + x * components + channel] = (byte)((x * 73 + y * 29 + channel * 61) % 256);
        return kind switch
        {
            0 => bytes,
            1 => bytes.Select(value => (ushort)(value * 257)).ToArray(),
            _ => bytes.Select(value => value / 255f).ToArray()
        };
    }

    private static byte[] Encode(Array source, int width, int height, int stride,
        PixelComponentLayout layout, JpegliEncoderOptions options) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, (PixelLayout)(int)layout,
            options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, layout,
            options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])source, width, height, layout,
            options, stride, TestContext.Current.CancellationToken)
    };

    private static void Encode(Stream output, Array source, int width, int height, int stride,
        PixelComponentLayout layout, JpegliEncoderOptions options)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, (PixelLayout)(int)layout,
                options, stride, TestContext.Current.CancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout,
                options, stride, TestContext.Current.CancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, layout,
                options, stride, TestContext.Current.CancellationToken);
    }

    private static int Int32(byte[] bytes, int offset) => BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset, 4));
    private sealed record Tag(int Offset, int Length);
}
