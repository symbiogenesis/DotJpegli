using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using DotJpegli.Internal;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class CustomChromaticityIccProfileTests
{
    private static readonly IccRgbPrimaries CustomPrimaries = new(new(0.67, 0.31), new(0.21, 0.71), new(0.14, 0.08));
    private static readonly IccChromaticity CustomWhite = new(0.34567, 0.35850);
    private static readonly IccRgbPrimaries P3 = new(new(0.68, 0.32), new(0.265, 0.69), new(0.15, 0.06));
    private static readonly IccChromaticity D65 = new(0.3127, 0.3290);

    // Independent complete profiles from unchanged google/jpegli
    // lib/cms/jpegli_cms_internal.h at 031a0077f5799a6041004267fc12b956c1f52a20,
    // compiled by VS2026 ARM64 C++17 /O2 /fp:strict /DNDEBUG.
    // Six both-custom RGB cases and three grayscale whites, all six transfer
    // functions plus gamma 2.2, with all four rendering intents: 252 profiles.
    [Theory]
    [MemberData(nameof(NativeCases))]
    public void CompleteProfilesMatchIndependentPinnedNativeFixtures(string kind, int index,
        IccRenderingIntent intent, string sha256)
    {
        IccTransferFunction[] transfers = [IccTransferFunction.Rec709, IccTransferFunction.Linear,
            IccTransferFunction.Srgb, IccTransferFunction.Pq, IccTransferFunction.Dci, IccTransferFunction.Hlg];
        IccRgbPrimaries primaries = kind switch
        {
            "aces-d60" => new(new(0.7347, 0.2653), new(0, 1), new(0.0001, -0.077)),
            "scientific" => new(new(0.64, 0.33), new(0.21, 0.71), new(1e-6, -1e-5)),
            "custom-srg-custom-d65" => new(new(0.639998686, 0.330010138), new(0.300003784, 0.600003357), new(0.150002046, 0.059997204)),
            "custom-p3-d50" => P3,
            "custom-202-d60" => new(new(0.708, 0.292), new(0.170, 0.797), new(0.131, 0.046)),
            _ => new(new(0.64, 0.33), new(0.21, 0.71), new(0.15, 0.06))
        };
        IccChromaticity white = kind switch
        {
            "gray-d50" or "custom-p3-d50" => CustomWhite,
            "aces-d60" or "gray-d60" or "custom-202-d60" => new(0.32168, 0.33767),
            "scientific" => new(0.3127000000001, 0.3290000000001),
            _ => D65
        };
        bool gray = kind.StartsWith("gray-", StringComparison.Ordinal);
        byte[] profile = (gray, index == 6) switch
        {
            (true, true) => JpegliIccProfiles.CreateGammaGrayscaleProfile(white, 2.2, intent),
            (true, false) => JpegliIccProfiles.CreateGrayscaleProfile(white, transfers[index], intent),
            (false, true) => JpegliIccProfiles.CreateGammaRgbProfile(primaries, white, 2.2, intent),
            _ => JpegliIccProfiles.CreateRgbProfile(primaries, white, transfers[index], intent)
        };
        Assert.Equal(sha256, Convert.ToHexString(SHA256.HashData(profile)));
        Dictionary<string, Tag> tags = ParseProfile(profile, gray ? "GRAY" : "RGB ", intent);
        if (kind == "scientific")
            Assert.Contains(";1e-06;-1e-05_", Text(profile, tags["desc"]), StringComparison.Ordinal);
    }

    public static IEnumerable<object[]> NativeCases()
    {
        const string fixtures = """
            adobe-custom-d65 0 0 89581A486DE58403950C5A66EA458EDDBA4461525764E25110BFF4AAC312B636
            adobe-custom-d65 0 1 2F369839E1B5FD3BC7F07C1B6CD2946F80E97730B30F07E38110A8CA37961DD3
            adobe-custom-d65 0 2 B6F11D921C16CBD79CD62D76C7A5D3F14EC36E5D39C3DF318CD3224A51AB707D
            adobe-custom-d65 0 3 107742C9A05948B80CF00D355E20BC992BE3DB1F7C2A84B377957D78E2B2A834
            adobe-custom-d65 1 0 640D572584EAF38858994F0DD827DC260B4A0EEBAC8646FF965F356A3F6BD302
            adobe-custom-d65 1 1 947CEF83E2AD18285C62BC6BE0F7C6FF024865A6F89DAE8C018E0A8802A410A2
            adobe-custom-d65 1 2 DE4942DFA639A0A16CD67FD1E45A91D7E4D42F07591589DEF686576946F5C394
            adobe-custom-d65 1 3 DA480D2DEE65AE186F4A9F4B3FAC013A3D8B9F6F6835404D149EE403448EE582
            adobe-custom-d65 2 0 E2572E4FCADFDA538C361E5B53E037D93C8EFF574DFFCB2F16DE62C100B7CF0F
            adobe-custom-d65 2 1 A33B490502AB6E4033FAAF9296814B74F71942BF1DBF0172EC65D5F9CE72D90C
            adobe-custom-d65 2 2 84AC385D337289F7F243BE3BCFC9D256B872B09438225B5635A76D082FE86D09
            adobe-custom-d65 2 3 C5BE97CD98A42836DE4A5B61BE5EF13923F0E6C2AADFB1DEA27160C89FD61F76
            adobe-custom-d65 3 0 31212C8307E1406EE2F8F141E8271079D8FE85C80EA4BE2E4720CF26295C0FAE
            adobe-custom-d65 3 1 32CC5B90828C3650B8F80A718D81E987957092602EF1841AD5BE40A8AB6ED308
            adobe-custom-d65 3 2 A5200A6214D21350B0761DE3271D51F2CAA633364F5B3F2CB3F3D1F43A907FA8
            adobe-custom-d65 3 3 E5E2210AB2400CFACB676A3977F7A59976DAD5AD4379ED9813BBFDCE5B4F7AD2
            adobe-custom-d65 4 0 48E87C72569C1A14D3DA2FF6DF3C4580AA54AB2A1624FDD4FAA906B411C06C63
            adobe-custom-d65 4 1 2702EBB07901DE90AD2F45050FAF7F887B259C22B393360C4504991064D23A42
            adobe-custom-d65 4 2 2C1822637D52CBC88ABD45170783A57DEF4B53BD2F5B4BAF4B070962C877BD30
            adobe-custom-d65 4 3 1917B61B99680CD6D3F4CFC64E810A4DD6A34C311497BBBBEF066B5EF5423BF2
            adobe-custom-d65 5 0 E0FC3FFEA1D71E31CC8DD0D727301D638648A1BADCBCBFFE95459C10CC176533
            adobe-custom-d65 5 1 1004D209F43A9C2761FA316177F65A48184F85C46F686B33C2CF2451128C8A99
            adobe-custom-d65 5 2 2414557D46B57B1447DD247DB227BA1A5D037CE120D1040DDD53C3982E072019
            adobe-custom-d65 5 3 1F08347E5B768AB3A3AAB036F76ACAD610C0E882954B2E4864DA0D43F8A490ED
            adobe-custom-d65 6 0 7FA62CB652DF58F8AA9B7AC8D4C835F59E7BD0E26F5694900ABDE394AE32C12E
            adobe-custom-d65 6 1 F256C731DFBFA4133A32BD885F1A1C353D9F8AF90BF7CB66F0A70BD62986C122
            adobe-custom-d65 6 2 CD63D5217DA144CB4DEEEA4B17C64033733D585AF14A3133335D5018F460DD6C
            adobe-custom-d65 6 3 118CABC2CA73341177E7F5B5A682C1E91BFFF544D7771185925E2D0B20018282
            aces-d60 0 0 8A4AA452AF8595DA4EBB0ABBFD9AAD94F0C60E40BB5B4B166089207B0016070F
            aces-d60 0 1 9D42479F7727DE814AF696F751E2D4822B2B736D962C17FC6B484AAAD1F477EF
            aces-d60 0 2 1FC3C23A52FEAEB772E436378397DED312B6E3D962115831BDB06549F7CBF402
            aces-d60 0 3 7650692082591677DD8FDD184F64EFAE6746DC6E4BB5970B116C8AA8F4F10031
            aces-d60 1 0 30584C330CF069671FDDCB629E026AC94342C22FF7915D8F09462D0263DD6900
            aces-d60 1 1 2A80300A78FCD72CA8ECDA3EA3B68B048D9E9639DC37F703BEAE5F7CF5CE6540
            aces-d60 1 2 D504B2EA13B32D5BE6EA6F5DAF464869096383367C026923F2E66032B39B887F
            aces-d60 1 3 3A86C6AFC2FBDAC79ED54CFED6D25D8708D176FA502D414C4DDC66A4223592F1
            aces-d60 2 0 0BE004728C85AD4688C27A935057226F59F1619B475AE4654ADC188D86F875A0
            aces-d60 2 1 769814DA013DCB0351418180D7ED4B6A9D1B5B6D5B8DB4E566FFDFD4EAD0CD55
            aces-d60 2 2 694FA4629BBA44A65BC049005FE21CA1DBC34F3CF62419BECDEB99DC513EF07E
            aces-d60 2 3 30D01CC0AABBFB3FDD258760054BAC65AAAAA193A33DF0C72138BC6FDB72977B
            aces-d60 3 0 EC22566EF5576BB833AE451435100147D894104580B2B58BE629C8C71C58CF43
            aces-d60 3 1 9DE614FAC27C11DFC50CF64401B430CB8F7391ED5EB7F222131CA7F98C096BBA
            aces-d60 3 2 DA7DD5936D1264B9FCBDB22BFEEBE1FC0D6C07F8D38EF41060A99695D59476E3
            aces-d60 3 3 D72BC8860CA45FA60B8B8CA76B430BA8A15D5CDD7691D74F2F1004A9D6609DC3
            aces-d60 4 0 FE90CA77FB450A68D3F2672DE65E5E75BCB8AA213EAAD78961BA3A233B992552
            aces-d60 4 1 4869A4F5D93374639887F209CBF7529F0D15CC0AA85624C049A9BEE1BA0D8BCC
            aces-d60 4 2 AC84CCC123267C8CF37E50BAB23B68F340BE8D747D66095E979D69E71AD9ABE2
            aces-d60 4 3 F3C72F054C2F0E5E5C594381CD5A111E0CB255E5AB3B18C075DC2AA6690DCCA1
            aces-d60 5 0 7840CA3315077FC9D4DE892857DABC1FC2BF0A93509B4EB6FC403359C9617A20
            aces-d60 5 1 096B12492D37E64C33A37515BF1B3DA6B76B484513EF6F07EE1EA39BEFE28152
            aces-d60 5 2 0ECFEBA0C24A3999BF265D7189A774DDDDD8A3227C8B91BD16D19F9BE475DED1
            aces-d60 5 3 CDE14F60D65F65EE1D05BBD6A9E7D6C1F3FB1B9360D0C1D8CCEA763B308DC98B
            aces-d60 6 0 83EC58295F7293E0B551C3D70223AD7F08C876F7A0105EB9F88EC723EE8289C2
            aces-d60 6 1 02208AD9D852F5D4F6264336B256544FCDE60E44E7822CFC6CB30DE09DDCF1DF
            aces-d60 6 2 79F4942FB0AF57145EBCC92AC4A810CA478A5281D1981C71212DF2243E2E1B1A
            aces-d60 6 3 3C76146713A03A7D2FBEE93A4803D5D05D995ECF1425510631133CC2423C6CE7
            scientific 0 0 3B54A376FC91C3C5A6990F4B17F475F1F632FE16CCA9E4B31BFF2B1C484DD6E1
            scientific 0 1 9E36260DCF8B5EA0A20E4488754CE442A77A2EDDFBCDAFE103872B34EB5BA5BE
            scientific 0 2 7E7D3FCE328FBA9D991463A5AD5834655EB59BDAD3743D652F8111F277F06ED0
            scientific 0 3 55E763DEB9B138A6AA6AACF824333965542F39E437DFA649CDC81F19659BAD23
            scientific 1 0 C6B5F981E567547063E72D35ECBB5A1E35D0C80FA3F01E7DC30C75A0548CA57F
            scientific 1 1 1C6940EFE6F2B4FC67004F8A890AE02E9BD8087250876342D884FAE7CBD62FA2
            scientific 1 2 0A99D4EB9EBEE73A3CEB9E989A39F1AC60C5AC972BCD6B5BAD9520A4D157204D
            scientific 1 3 D354AA5400558ECB1B22601D62ED0A252879B2D0D3778965E3B360DA265796AE
            scientific 2 0 D8DDA9A7AB6001A74FD075E32D549EC03D9A44450A885F0B81DB7779F82EFD82
            scientific 2 1 6B545E087472C1B2F14BD2B4B54B47E0DC5A53352B3285B2DE4EE820AE3EB9BB
            scientific 2 2 CC97ECF76C39E5177CC42F4889F7F5D70F4D5AF7926ABA1A56D38150EF51EFE1
            scientific 2 3 EADCD14BE2D3A65BF07BFC44217138D702B83313DE8B9DDAABA6112DFAEBDFE8
            scientific 3 0 50D6A28632C24900F66BB3DCEBC4F175164430903D03663006708FC70269125D
            scientific 3 1 C8D43921E5F5E6ACA2100133AD59ADC167EDE96CA3229C0BDA5B350110693868
            scientific 3 2 7097708D24146D51EB7554CA27814BB51EBEEF7DCB2889629F2E2DFC556D7505
            scientific 3 3 A5BA15E7312053ED6A79C02D65CF41E75D1BBDC03A3067D5E3B1901003560563
            scientific 4 0 8979D7C99DA7F5AE2F203A0F7C47CEF41111ADBB58D21B1CC14261DF9D9AAA56
            scientific 4 1 FAE74873A8BFAB88E5E50E4171634E2826D736590DFF7CD7CADE1DF7102DFEDB
            scientific 4 2 FE7C95AD1663B1F7C5BE62589B49A0AC41633309B964A15E4F053FDF94D30C33
            scientific 4 3 0F25C5AA23C1CB09C3755CA84EE0CE868450DE3A98414D8A257390480FFC2478
            scientific 5 0 E337C8AC4D4DA428FC6AE42B3D4FE543073B948CFFE150215234AB8DB6678654
            scientific 5 1 6759626B7F729A0B3F47F11FDDA365E3980EEE23B530A769012168DEEA1A884D
            scientific 5 2 A077FD83A71AA5152A7E0567A7C3E068565EFEFE98C4EB710019EA933242F59E
            scientific 5 3 5E9CDB2702B7993B7EAA0E863794BC185DCDF8524B5216DA34E95365AE7EBE12
            scientific 6 0 A07A26FFCD568045681FAEF8E0C03917AA3F5F7D6B40D469254FE74264C45DE8
            scientific 6 1 777B9F2EE170E4F0E61C886745A2A1A491D6B56956F365B9E1F191833063C56E
            scientific 6 2 E58604B3F550DB181F485C102F4D1798FC7CB48D331ADC3708DA0459302CDFD1
            scientific 6 3 DA085D36685ECF4DD30D9A09393B22146B5AA9DCB6A4B2A8A26D917B14905946
            custom-srg-custom-d65 0 0 0FC5FC368172272D43404A07E5F781CB6325D81599C4394192C4C4D0B6CDC884
            custom-srg-custom-d65 0 1 FE322E29D5BAFE7D7152EF9DD86DBD0F8E0AF0EBAC474EB579C81D6FE10BC87C
            custom-srg-custom-d65 0 2 AB348D087C578C7D9CB80966EA498BDE3CE94C093A92F6E64BA0510E921F2683
            custom-srg-custom-d65 0 3 282A33CC02801B66C28210416B49945DFFA09B8C9B1B8EEE7EC8D18A23322907
            custom-srg-custom-d65 1 0 8DC3498FECC02FC09392F6B653B0A0D7D7904B9BD4FD6EA886E000E086EE55AE
            custom-srg-custom-d65 1 1 CE4679E42DDF48DACF252F40564159B7CFC0E2A30196216776E54FADA9B86495
            custom-srg-custom-d65 1 2 F8FB896B1E7BF51F1DFE6F8203450439006CEC5295D0ADA5489C8C1728C98278
            custom-srg-custom-d65 1 3 51E00EA38311626EB0AC220BE284C8563EA9E41B0FC029684FDC4BC12939691F
            custom-srg-custom-d65 2 0 E183503BDB3F3B259A2D810ED0B02117E263424E2ED1B83B1C4E609CC0C76437
            custom-srg-custom-d65 2 1 4D610DCBD2A77828DE6D6E82BEE5729EE2C2899B4A00E954BB0A9256DDF714CD
            custom-srg-custom-d65 2 2 D9920903DDEF3E7E35A8A776008BE190B7A1F40DE81B46685EDB3C6046961026
            custom-srg-custom-d65 2 3 DFB2975FEB85432A5E98EE31B27CC9686BF76BC72155A372FD0C18AF7AF1F9E3
            custom-srg-custom-d65 3 0 B843F98D47F5D66524E342C0BC0B59C7A80DCFA6C62EDC10269848B1BE550B48
            custom-srg-custom-d65 3 1 3F03815CAF184C98F3C3829767B83C9F8E2D88C51C0E6AE41C9C5272AA4DA624
            custom-srg-custom-d65 3 2 C616025B54DAA258783FDC2C9E7D07531FFC91A34B920A18FEC83005CA796369
            custom-srg-custom-d65 3 3 8DA352376F66681B1773D8D9B11AF60D5159A7CDAD0D4187D8FDD4C7C99A6590
            custom-srg-custom-d65 4 0 CB291322502CE6C94C3E55A199CB19FA8CC8A2C6335FAFD0DB317C02E41D3A04
            custom-srg-custom-d65 4 1 F50ED21B4902895D7C6538AEEAE3A2D28EF293C1DAFED79BBF21E89A647AE409
            custom-srg-custom-d65 4 2 189A0E578E34FF858324F0F1C0DDCDF56157873C92886342EF403C72BFCAA512
            custom-srg-custom-d65 4 3 E858651BF4FAD8B4415B5805022B0711B5F37DA200687B10CFD9713D17FD6D50
            custom-srg-custom-d65 5 0 62F9B3D949E4826F7C095CEFA1129F9B262D830CAC287AAE76B5FA4EC7290081
            custom-srg-custom-d65 5 1 43251473A70C532B02C147086558CD323121C661FB67116854FBFCB1BE70E4C4
            custom-srg-custom-d65 5 2 8DF5C751D9BAF38D9ACA01B333D416483D1E90E62D73554F2F69800786D514B0
            custom-srg-custom-d65 5 3 B523EB9DC393DE09B160A93F207F7AEFF3CB7F36B0C5AD675D6B88793F2B6861
            custom-srg-custom-d65 6 0 F9A628C7CAE9EFE96500F2B8B40E0FBE26841D58913F6DBAA35FEDFD0B46FB93
            custom-srg-custom-d65 6 1 3ED336167FBAA651A8E93BC9D749DAA27CE0A9341DF7DA92A05E30F45743D227
            custom-srg-custom-d65 6 2 CBFB18C1DE7D463FD7FCD9E180A128BEC5B7F1464A4FCA535606ECF1456ED59C
            custom-srg-custom-d65 6 3 D8D09E87CD81243441335A06A3F0209036BA3C13352854251C602A10F561F650
            custom-p3-d50 0 0 4D33D0D8D82741549FE7A0DA71F3538BAEB605DEB231C58799FD02B221636F41
            custom-p3-d50 0 1 9F983C4537FC5916BAE5BC7DF588CC1311271A357FF84932812DEFCB9A89972F
            custom-p3-d50 0 2 1FC3E25859E779D8016BFC0571B331B8523DCA6AF77204F4BA324C6D1B923AF4
            custom-p3-d50 0 3 C31E6B8E83568842D063DEB0EE0828DCF851A011EC434FC76F23465010BEAB51
            custom-p3-d50 1 0 B3BC4A4B17C141558D1EF91CB90C658C452101B53E3956A3CD05A5D34D5B8D0D
            custom-p3-d50 1 1 F63A5D9CFDE26D5B67894A8175F5B814A7151435684D21A5E14C9482EE14FE56
            custom-p3-d50 1 2 F82B2D02AD446FBE99FB77E4290CC57F96AD9B6F127FEC6CD00EECAD1E8AE4EE
            custom-p3-d50 1 3 C20DCD0629DFFF6E502E555727AD57AF1BD669CF22B05A2BEE5C22E1BAA27D63
            custom-p3-d50 2 0 7CF03B2E3AFD75560789BA75B6474BD5388582BF5278A2F00AEB29D7031069B7
            custom-p3-d50 2 1 5036C46D457F6D64AF058DC1C84AE87FD9C5D46FE68FFAB1D0B30DF9F91CAC62
            custom-p3-d50 2 2 8F0C6430A57AE355F1B26BEC07FC87E483516BB9B4A5E549B23B68293DF09DAC
            custom-p3-d50 2 3 0BD098D57FD60B104EFE237694733D5C198AB6AFB24EA91477588FAEB83693D9
            custom-p3-d50 3 0 FD6BF5F840AC77BD75DA6ABAB09774D174CFD87A2C607CDF422C4AABDDF93164
            custom-p3-d50 3 1 0F19D0A08D109253AC8028FCC6DC03D31BD139BCD6B7715C9D187C3E77014C48
            custom-p3-d50 3 2 604AAE49982F4B214ABF9C815811ECB82A565E5270D7A379C63DAD4EE2B70607
            custom-p3-d50 3 3 88A47D9B9C3E41B819CF95E0ABDCF0CF3C18A08EE062A69803F58C33CC0D5198
            custom-p3-d50 4 0 53DC596D5253180DE1E75119D126CE371E5DB8204673F1534DE5D40699994A09
            custom-p3-d50 4 1 1BE6725646DC6B100BF2147F79A9D09EB295479D1436A64566A91EEABEF8A0D7
            custom-p3-d50 4 2 BFF514C7EAA3B06FB04DCB592EEA3E48DF4FFC46C59EAC119E4523F3AE9F55AB
            custom-p3-d50 4 3 0C294A7AB2CBA2E5E2E7910C40E8439CF834B571F7CCEAD14AB04B358D9FDD3E
            custom-p3-d50 5 0 099B9A7252FF2CB35D0633BFD00E55B1E124F3EB5C1A8334B6803818B1EE2334
            custom-p3-d50 5 1 74BC3E6F3AA74ACAEF922F708E2A22DC4D4B86A94374D27F026F1129F5AF117B
            custom-p3-d50 5 2 DA80BA86690CBD4C1613529374627E87B087B1B3AE06BB57B203C065E9C2EF58
            custom-p3-d50 5 3 FFC1B2F958E43A9E8603AE30D023DE303CCE1753ABDF8FDAEED17434966F331A
            custom-p3-d50 6 0 C17373F84A85261DDC517BCF5D55266171BD174A893A64C0B217DCB920D6196E
            custom-p3-d50 6 1 36CA41DEF06B2A1FD4400FED6E01BEC3D9CE45264FDA1B2CAEFBAB7EF4A35AA0
            custom-p3-d50 6 2 CF56358C35125119A84361696324A1EC73D8FBA302613D2628F465E4874E0F01
            custom-p3-d50 6 3 C8154AAA3A9F7FDC56C5CDE2C393D56478A432580E59F0EF7AD719D45D75108C
            custom-202-d60 0 0 3832F06B2182AF2C8A6BF6BEB63009106992857D63165B6F494A0B2E83D8079F
            custom-202-d60 0 1 D3CFCD42C8379E84BF2A4220C2AD4E2ED241D10172533DDE4ABED0F24AEE11D2
            custom-202-d60 0 2 73F46253BD45514155EB626953066670AD150878C9A066D5A0C091CC38D70BD9
            custom-202-d60 0 3 11F9A6FB60137DF7BCF4F8D6F6D5DA2F0956203815B03B35455E2CC751FD32FA
            custom-202-d60 1 0 7A6F086DE660886A68014FC04F2B9378524280C96693D61A578F0FB0FDB28485
            custom-202-d60 1 1 C91A33AFB3DA5970C6D6B0CDA9A36DA3E71CD40636DDDCDCD9366702EDFC694A
            custom-202-d60 1 2 4101FA62FEABA3C0275A8D2EBA2EE6425212EB3089C275D106F1D953885F16D1
            custom-202-d60 1 3 79EE9381C7D8F40437B19199A8A6F184AEFAFA3311BA029ECAB3C1A43F85EE75
            custom-202-d60 2 0 158FFB96DE9A7AFE0C916624E93AC69329CD4FE1DE4CDF2FDDFD0F02C809E451
            custom-202-d60 2 1 FD16F3C305581A256851B7FE3278CF605EF4F3760334F113C71933CA12D4E5B1
            custom-202-d60 2 2 7809E4814A7EEEAF869698FEFDCDCA0FBFB27FAF9750E869E41BD8C37B82EB88
            custom-202-d60 2 3 4A19FE46B7BAA2675F58E28B01FF2D24EED8CD7A130A1EFEF9B00F1D9421A1E8
            custom-202-d60 3 0 3B82F32143865272BF5D6907F39383CED89BF832F2A9109C43B5F6006DFF6260
            custom-202-d60 3 1 E0C5E13F49BDBE80EEE0FFD1AC34BC6CF3116D47278D42E135BECD13170D8EB3
            custom-202-d60 3 2 28FC890258CDB3AC5A28F5A21248FBF01D2FF11A00B8C27646F759A048480F83
            custom-202-d60 3 3 327B977A5A2483D1E447EAE77DF243B55DF9E2CCAB000736486E38B25282768E
            custom-202-d60 4 0 5CE5FCA06A843D162AD9CD80D982F4AD3C0B5B880306D7D401A6CD76F333A2AE
            custom-202-d60 4 1 C0B8F0CA08A9301B8D66F561B034E436769253972DBC13062975301E5D295FD9
            custom-202-d60 4 2 1F2F94617107C6300BB041AA5B5C4096BF41C31806946362F75CB4A182E3F500
            custom-202-d60 4 3 5C837DC36E2805501E3AFBA93B0F2E9CF596F3B8215B0E179A9F4DFC88EF259B
            custom-202-d60 5 0 FFFEBFA5029484DC82F9AD137111D901DEFBF553C28F221601C5472D349AA687
            custom-202-d60 5 1 4512B366662704A1890C538B465A39E59FB559ED90B7865217BA0D20A544BBF1
            custom-202-d60 5 2 B7209246FCA52CB5127DDDD47C0731F3772FB05734B26229BE07BB8D586E5479
            custom-202-d60 5 3 10CE6097AA3C6C943F7784BF31D03EF837D4FBEC96538EE7F53BB47E1B39B318
            custom-202-d60 6 0 4E421605A198654BA43A69731C43E5EEEF46783B4B861BE18B200006B030EA08
            custom-202-d60 6 1 710AE9F7F33AF8764E79DF6138921F8AD87DC2DCA8C6DDD94C93096C5E5EBF5B
            custom-202-d60 6 2 AE836F6CA5987066618B823F729FBFF0404600F0AC298E70377577EF0AF2986C
            custom-202-d60 6 3 CA9C610461D53DA8334FFBB1DA339574ECCEBC3F735B377FA42908590C40EEC9
            gray-d50 0 0 C03236AF92A68E6E7BD5DC3BCBA6D831A966A71F1A2D8B2786FC6788266C6EB4
            gray-d50 0 1 472901664EE2EB03093F3EF33A0ABDC07D691FCF1B9C8FEDDD7CBF0EA30D479D
            gray-d50 0 2 00B4EAC4A886F321FFA9705EB8D0D746C7997300007BE35201F7007D01FA4E00
            gray-d50 0 3 CDB646E7700A782E16D23A9A50625C0D29E81F351BA7BB98DFAAA5DEFFD554F6
            gray-d50 1 0 113ABC23B39B7691BFDA5FE1B05C7136E9038AEB37273DFDCB0DE7BBF2F09764
            gray-d50 1 1 906D6DCF436A2860B46FD3CE55EB68783A6B53BB69422643317A1E8A2827F740
            gray-d50 1 2 C453D1CE84BE3F215DF1D5A7DBFFE871C1692222EE8453267230D7B47E1EA110
            gray-d50 1 3 567A39424EB940104A31E1A0ABB3FD1BEAB7F0C6AFE0B35C4373744581DFBC69
            gray-d50 2 0 CCF6F8FA7AEA085FEBDEC44DEE0BD37837250ACD47F1D80259E0E13B345D7502
            gray-d50 2 1 D7F6334E58A26939C5738FA37861377F63E89F9F7BE88EF75E5EBD25CABBBD30
            gray-d50 2 2 F0C2F10A35B4594C98AB0E15EA5BF70B78CC2B1C9E7A8B19B76B43A04689C78A
            gray-d50 2 3 1E9AB3CB59D90E7CC8DF9A1680158A46DC9ABF25E19C6BD166A5B171472A3F83
            gray-d50 3 0 9613B026D83E510A1AFD5568FC16CDCB17B667D135E51CAC2623D1BE7C7F1C82
            gray-d50 3 1 BB796462358D19FC5D7AC317C3CD91AB10E5D502DE18578A6C92232D731676E0
            gray-d50 3 2 08F4915DD4DA3354A467980937C1B699EE8622971F70B1584ED2025957E39E6C
            gray-d50 3 3 B3BF26ED1E4CCAC10C826F59450598D0AD9513FD6A15A66FED8ABBC24AA45613
            gray-d50 4 0 D10FA678ACDF699A85E8B9DC2CF936783FD07C3231B1DA6C198874898C32DF62
            gray-d50 4 1 C21DD60679821714F968EA194F5E7322907C20F55BDA025B96B0A5F35991B7DE
            gray-d50 4 2 27FC54A309FCEDAB5447412341532C85F73180250A152DF13B16594A27D91E9F
            gray-d50 4 3 AF96CFBF658B394C4095523FD75BAF2D6ADCE59E87A5AFF503B2AFCA900CF248
            gray-d50 5 0 0E7479406DA6E756396D967B8DAC5EC6E594BF047AF6582A458FAC8CE6AF27C8
            gray-d50 5 1 755497D9BE8C09BD28766E70BD17C0A83FF260FCAFC124C30BA72F519D3E0067
            gray-d50 5 2 D4BA312D875C278DD7D94CBF570393F83697FD78FBC5DB11C6515AD934B958BA
            gray-d50 5 3 2C7D759839321778A0613E4B8AF1DA2FC51BE5B70F182C8A793B5C412FC41CC7
            gray-d50 6 0 DF0FE56C12C1C23BECEF3BDB086162B3196F0C09EC409D1BB3A3F13F9E1E050D
            gray-d50 6 1 38CF95D74443BB18FB37AD2097815533D67A3D8D90499D6B7B8700CBBF6366CE
            gray-d50 6 2 96EC9B8E9B3940A965C71E7DC598E3B4A9015BAFB17D8E5D8F3E79611C2E3C30
            gray-d50 6 3 C2E848B16EF6BEEE5619AB40E599F1454C5B308BAF96815E9089544B764E8417
            gray-d60 0 0 5DB1C53448DB1410A221AB6C88D3CB57328F8DA561C4C1027DF43288EAD1DA98
            gray-d60 0 1 ADC796C74B9EFCF8E1D90E50DE165055345D733A00C18CF20102CBB55D1321F4
            gray-d60 0 2 A0496287AFE9D201A5C3CAE4BAB7D4497EEFC765DCF795F5966AA29C115D4CB7
            gray-d60 0 3 D1241A26A3B2E860090477E7431CFC6C775950463D113A5B450C847DB701CAB7
            gray-d60 1 0 5BDDB93178B497CF257E49C8EE5F528531C18C48CE1997288F526E49CBEADE7D
            gray-d60 1 1 89AAB930821993D80CAC1E3E82AC669D948C32E46D5782605C2443D548B305F3
            gray-d60 1 2 0FE9B59D308A596C1EAEA93424CDDA10CEAA79C39462C70A3500721E78629584
            gray-d60 1 3 83845A3D55E0CC0C2E7EAF4C7FBA82A687A6789531FEF3D6C93C3F718DAEA2BA
            gray-d60 2 0 1651C508A462C1F3887BFB9FF31CC07109E0CFB9DC520C7B296F1F0ADA215179
            gray-d60 2 1 567A0A17D5F7EDD35D40DD3834CE9C792F021DEFA8361210217A05F77D8668E9
            gray-d60 2 2 11E4801477800FDAF1763320DFE82354D2647AE04922E188985A9A6F8883DDE0
            gray-d60 2 3 C9B6312043A85EDABD199231D9D589EC336B550C9CBA9A48608986E9512CA303
            gray-d60 3 0 385A12F4366BE05985DE0B12927B78F634932341627E0AE3E200FE02774F80C2
            gray-d60 3 1 E56369D5EA0498D7C6071D9B1878E6C5F12C5A33C49EB5D33BFD46D891B0B493
            gray-d60 3 2 C73C65A728EA8289E6012C0E93B65ECAD1B374A134926A57259A40CCC65A95F9
            gray-d60 3 3 DE699A904ACD3BDBE20B51BA3AE07D9561373975113491CEC32C0BF23B4D0C93
            gray-d60 4 0 9514B5FA8CFA4B608C6419C74D6A4094B7B33AF843554083687969F3A8F3034F
            gray-d60 4 1 A22DAA9F39850AA6DD4AFB31BD286256CE91587217F8F03BB574E2B145612D1C
            gray-d60 4 2 DEC8D7ECDD297B34786FDC0E385EFB282488356F40334F66A7C2F701ADC8AF65
            gray-d60 4 3 E154AC21B629783F58B5D4021AF65930B62B5A5AA2BC6C7E2A763DE0367D736E
            gray-d60 5 0 3603201A86A3652E6879D8B261586451AB304909D0250A2383B7AC7EE0210195
            gray-d60 5 1 90139C36E08A8035EC54AF4F43A19936C7371B0CC5F018A70CC501C3F6D552D1
            gray-d60 5 2 5A6125C284517C29AF23400337DDEBD2950D238254B210106D12C6A69498A3A6
            gray-d60 5 3 EFFF60C8579BD6F0E239C1A16A084EC3B175A4C590A9018BEC8849113A9776B3
            gray-d60 6 0 89E1EB91A9D2F5C8945704F7840496409C742E1164A6FBC1278D1B04F37526FC
            gray-d60 6 1 4F952D716B7D7C98201F91B38F8163A3FE0AE4659E3EAD6C2CCB7B053910BB53
            gray-d60 6 2 B241ECB86EF36FC21891C9F4638541765FE0526A785F7A0997CAF9DDC1B351B7
            gray-d60 6 3 2AC0B1B42560B75A92B352D22747204D98BF50B4C79FFFA130848D1217F8D00D
            gray-custom-d65 0 0 471298B39233F4394751BE6B667B178FF2CF6637B6A0B9944A1A02FACA63CBBB
            gray-custom-d65 0 1 6180EA8FBAB6B45907F794C6B7E0A9D13FE01618FBABB78B0D098079C4025BF2
            gray-custom-d65 0 2 7692BB21EABBFB5B0862CDAAA3450A0F0C486667C62B85BB6DDDA31291D07EA1
            gray-custom-d65 0 3 10720EB994C52E03094517AEEFF659DD5E857D186B602E5D3FB6B01761142AB5
            gray-custom-d65 1 0 05E07C72283873C30B6DE5043F4B97C3C1F84E07911FB78DF64EB0F115717089
            gray-custom-d65 1 1 F634DBE719C74B85C3D59A61F29F62AA6240793AF395AC907ABC843280AEB9FF
            gray-custom-d65 1 2 7782AC7D1713FF4FDDABA5FD2062E34A1C36EC51D42AD93448FAEFF75B6B3887
            gray-custom-d65 1 3 325860EFA55081318A4781FF120ECC6D08826E3C71BD21C66B7FB054A1266D08
            gray-custom-d65 2 0 95A516F47460686D5920564EA2BF5BE6CE553C9AB16C266AE18C76037F7DD72A
            gray-custom-d65 2 1 ADE4A2DD094E433BBF779FCFDE60E4064B4DC4F9B8F35E63D13553D04B464A8B
            gray-custom-d65 2 2 90941768C1294C3F8C190D9E686A12D3F9472A4F48F64AF4F14FF5580FF5D687
            gray-custom-d65 2 3 A761C8B2BE578BAA5A411870F61B136AEB173099FE201DF634CB79D7331AAB7C
            gray-custom-d65 3 0 389D2114BED03C6BB93600367EE7D1CD7D27A43208D56FBDEC0A6A137D45F2EF
            gray-custom-d65 3 1 4BA0EEE480EA34DD9F718A9EBC739344D444F0ED4E3A61CB2FCD96EBD73BF8BB
            gray-custom-d65 3 2 029D070263CB8192D956567A594AFAFF25B8D6E6CE28BE75FA379164C2C7E487
            gray-custom-d65 3 3 CFD48D1A280F8904D10EF29CD914E0BEBFD9D4D0D342F684AB01071455CCDE57
            gray-custom-d65 4 0 5A211F7602E4A06A62FBDFD3F811AB227135BADDC83B8C0D8260876D4B056869
            gray-custom-d65 4 1 65E1E3D512DAA81C7BCC62CDD4206ECEE3031CF5C8C2A72B574077E9A3A1C52E
            gray-custom-d65 4 2 37F25B07E38D2533005920AB2A3DF6929EC3EA3A24D18ABF8C484806770E7D6F
            gray-custom-d65 4 3 4AC57428DA0D46A86A8171B74A8786FC99F916009A12310B141804C8C09B03FC
            gray-custom-d65 5 0 053F4A07CA64CC86D9EE0FF4285125689986045BA26E27319E7E3166E3B52459
            gray-custom-d65 5 1 F52B07DE2E2F42490677D83B177BE72BAE938779198DE07BE9741432D9DCD5D0
            gray-custom-d65 5 2 EDD98F0587EE337240E8011D86450DFC98E0E30CEC37C90A22109DFD8FE32623
            gray-custom-d65 5 3 57DA8A07E3AC952CD1098B281E9CA65B7D08BF7FA75FE3BEC2FB52162D23542B
            gray-custom-d65 6 0 CA459CDF2BE78B5D2314E118044E720A30AF734BF714B2B6C645A930AC3DC84C
            gray-custom-d65 6 1 B4403AA3FD566B9C0FFD3FA5ABA5171582D129ED8665C492DCFF58A322B18CCA
            gray-custom-d65 6 2 73B7A5E887268D1529513672D36E296E1EAD19D256B58CC17E2B1229C661FE28
            gray-custom-d65 6 3 039D505D5014E709B5E44E1DB19A8737395EF4AFA4E7ADF879932804F9580BC6
            """;
        foreach (string line in fixtures.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            yield return [fields[0], int.Parse(fields[1], CultureInfo.InvariantCulture),
                (IccRenderingIntent)int.Parse(fields[2], CultureInfo.InvariantCulture), fields[3]];
        }
    }
    [Fact]
    public void CoordinateValuesAreImmutableValuesAndFactoriesValidateThem()
    {
        var red = new IccChromaticity(0.67, 0.31);
        var primaries = new IccRgbPrimaries(red, new(0.21, 0.71), new(0.14, 0.08));
        Assert.Equal(CustomPrimaries, primaries);
        (double x, double y) = red;
        Assert.Equal((0.67, 0.31), (x, y));
        (IccChromaticity r, IccChromaticity g, IccChromaticity b) = primaries;
        Assert.Equal((red, new IccChromaticity(0.21, 0.71), new IccChromaticity(0.14, 0.08)), (r, g, b));
        Assert.NotEqual(primaries, primaries with { Red = new(0.68, 0.31) });
        Assert.Equal(new IccChromaticity(double.NaN, -1), new IccChromaticity(double.NaN, -1));
        Assert.Throws<ArgumentException>(() =>
            JpegliIccProfiles.CreateRgbProfile(default(IccRgbPrimaries), CustomWhite, IccTransferFunction.Linear));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateGrayscaleProfile(default(IccChromaticity), IccTransferFunction.Linear));
    }

    [Fact]
    public void CustomDescriptionsAndProfileBytesAreIndependentOfCurrentCulture()
    {
        IccRgbPrimaries scientific = new(new(0.64, 0.33), new(0.21, 0.71), new(1e-6, -1e-5));
        IccChromaticity white = new(0.3127000000001, 0.3290000000001);
        byte[] expected = JpegliIccProfiles.CreateRgbProfile(scientific, white, IccTransferFunction.Srgb);
        byte[] expectedGamma = JpegliIccProfiles.CreateGammaRgbProfile(scientific, white, 2.2);
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            byte[] actual = JpegliIccProfiles.CreateRgbProfile(scientific, white, IccTransferFunction.Srgb);
            Assert.Equal(expected, actual);
            Assert.Equal(expectedGamma, JpegliIccProfiles.CreateGammaRgbProfile(scientific, white, 2.2));
            Dictionary<string, Tag> tags = ParseProfile(actual, "RGB ", IccRenderingIntent.Perceptual);
            Assert.Equal("RGB_0.3127;0.329_0.64;0.33;0.21;0.71;1e-06;-1e-05_Per_SRG", Text(actual, tags["desc"]));
            byte[] grayscaleGamma = Create(true, true);
            Assert.Equal("Gra_0.34567;0.3585_Per_g0.454545", Text(grayscaleGamma,
                ParseProfile(grayscaleGamma, "GRAY", IccRenderingIntent.Perceptual)["desc"]));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData(IccTransferFunction.Linear)]
    [InlineData(IccTransferFunction.Srgb)]
    [InlineData(IccTransferFunction.Rec709)]
    [InlineData(IccTransferFunction.Dci)]
    [InlineData(IccTransferFunction.Pq)]
    [InlineData(IccTransferFunction.Hlg)]
    public void RgbColorantsAndAdaptationMatchIndependentChromaticityOracles(IccTransferFunction transfer)
    {
        // Include a spectral-boundary red, a white outside the primary triangle,
        // and a permuted channel order. None are named color encodings.
        foreach ((IccRgbPrimaries primaries, IccChromaticity white) in new[]
        {
            (CustomPrimaries, CustomWhite),
            (new IccRgbPrimaries(new(0.8, 0.2), new(0.05, 0.8), new(0.1, 0.03)), new IccChromaticity(0.05, 0.85)),
            (new IccRgbPrimaries(CustomPrimaries.Blue, CustomPrimaries.Red, CustomPrimaries.Green), new IccChromaticity(0.2, 0.25)),
            (new IccRgbPrimaries(new(0.7347, 0.2653), new(0, 1), new(0.0001, -0.077)), new IccChromaticity(0.32168, 0.33767)),
            (P3, D65)
        })
        {
            byte[] profile = JpegliIccProfiles.CreateRgbProfile(primaries, white, transfer);
            Dictionary<string, Tag> tags = ParseProfile(profile, "RGB ", IccRenderingIntent.Perceptual);
            Assert.Equal(new[] { "desc", "cprt", "wtpt", "chad", "rXYZ", "gXYZ", "bXYZ", "rTRC", "gTRC", "bTRC" }, tags.Keys);
            Assert.Equal(2, IccProfile.LookupTransferFunction(profile, TestContext.Current.CancellationToken));
            Assert.Equal("sf32", Signature(profile, tags["chad"].Offset));
            Assert.Equal(44, tags["chad"].Length);
            Assert.Equal(tags["rTRC"], tags["gTRC"]);
            Assert.Equal(tags["rTRC"], tags["bTRC"]);
            VerifyCurve(profile, tags["rTRC"], transfer);

            double[,] adaptation = Bradford(white);
            double[,] expectedColorants = AdaptedPrimaries(primaries, white, adaptation);
            double[,] actualColorants = new double[3, 3], actualAdaptation = new double[3, 3];
            for (int row = 0; row < 3; row++)
            {
                Close(new[] { 0.964203, 1, 0.824905 }[row],
                    Fixed(profile, tags["wtpt"].Offset + 8 + row * 4), 0.5 / 65536, "D50 media white");
                for (int column = 0; column < 3; column++)
                {
                    actualAdaptation[row, column] = Fixed(profile, tags["chad"].Offset + 8 + (row * 3 + column) * 4);
                    Close(adaptation[row, column], actualAdaptation[row, column], 0.00002, "Bradford adaptation");
                    Tag colorant = tags[new[] { "rXYZ", "gXYZ", "bXYZ" }[column]];
                    Assert.Equal("XYZ ", Signature(profile, colorant.Offset));
                    Assert.Equal(20, colorant.Length);
                    actualColorants[row, column] = Fixed(profile, colorant.Offset + 8 + row * 4);
                    Close(expectedColorants[row, column], actualColorants[row, column], 0.00003, "adapted primary");
                }
            }
            double[] adaptedWhite = Multiply(actualAdaptation, Xyz(white));
            for (int row = 0; row < 3; row++)
            {
                Close(new[] { 0.96422, 1, 0.82521 }[row], adaptedWhite[row], 0.0004, "adapted source white");
                Close(new[] { 0.96422, 1, 0.82521 }[row],
                    actualColorants[row, 0] + actualColorants[row, 1] + actualColorants[row, 2],
                    0.00005, "colorant sum");
            }
            foreach (double[] encoded in new[] { new[] { 0.0, 0.0, 0.0 }, new[] { 1.0, 1.0, 1.0 },
                new[] { 0.125, 0.5, 0.875 }, new[] { 0.9, 0.2, 0.05 } })
            {
                // Isolate matrix correctness from the coarse HDR curve grid.
                double[] decoded = encoded.Select(value => EvaluateCurve(profile, tags["rTRC"], value)).ToArray();
                double[] expected = Multiply(expectedColorants, decoded);
                double[] actual = Multiply(actualColorants, decoded);
                for (int row = 0; row < 3; row++)
                    Close(expected[row], actual[row], 0.00007, "profile RGB to D50 XYZ");
            }
        }
    }

    [Theory]
    [InlineData(IccTransferFunction.Linear)]
    [InlineData(IccTransferFunction.Srgb)]
    [InlineData(IccTransferFunction.Rec709)]
    [InlineData(IccTransferFunction.Dci)]
    [InlineData(IccTransferFunction.Pq)]
    [InlineData(IccTransferFunction.Hlg)]
    public void GrayscaleRetainsCustomSourceWhiteWithAnIndependentDecodingCurve(IccTransferFunction transfer)
    {
        foreach (IccChromaticity white in new[] { CustomWhite, D65, new IccChromaticity(0, 1),
            new IccChromaticity(0.19, 0.23), new IccChromaticity(0.8, 0.8) })
        {
            byte[] profile = JpegliIccProfiles.CreateGrayscaleProfile(white, transfer);
            Dictionary<string, Tag> tags = ParseProfile(profile, "GRAY", IccRenderingIntent.Perceptual);
            Assert.Equal(new[] { "desc", "cprt", "wtpt", "kTRC" }, tags.Keys);
            Assert.Equal("XYZ ", Signature(profile, tags["wtpt"].Offset));
            double[] expectedWhite = Xyz(white);
            for (int row = 0; row < 3; row++)
                Close(expectedWhite[row], Fixed(profile, tags["wtpt"].Offset + 8 + row * 4),
                    0.00001, "unadapted grayscale white");
            VerifyCurve(profile, tags["kTRC"], transfer);
            Assert.Equal(2, IccProfile.LookupTransferFunction(profile, TestContext.Current.CancellationToken));
        }
    }

    [Theory]
    [InlineData(IccTransferFunction.Pq)]
    [InlineData(IccTransferFunction.Hlg)]
    public void NumericBuiltInCoordinatesKeepCustomHdrTableSemantics(IccTransferFunction transfer)
    {
        byte[] custom = JpegliIccProfiles.CreateRgbProfile(P3, D65, transfer);
        byte[] named = JpegliIccProfiles.CreateRgbProfile(IccPrimaries.DisplayP3, IccWhitePoint.D65, transfer);
        Dictionary<string, Tag> customTags = ParseProfile(custom, "RGB ", IccRenderingIntent.Perceptual);
        Assert.Equal("XYZ ", Signature(custom, 20));
        Assert.Equal("curv", Signature(custom, customTags["rTRC"].Offset));
        Assert.Equal(64, Int32(custom, customTags["rTRC"].Offset + 8));
        Assert.DoesNotContain("cicp", customTags.Keys);
        Assert.DoesNotContain("A2B0", customTags.Keys);
        Assert.DoesNotContain("B2A0", customTags.Keys);
        Assert.Equal("Lab ", Signature(named, 20));
        Assert.True(named.Length > custom.Length);
        Assert.Equal(2, IccProfile.LookupTransferFunction(custom, TestContext.Current.CancellationToken));
        Assert.Equal((int)transfer, IccProfile.LookupTransferFunction(named, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(IccRenderingIntent.Perceptual, "Per")]
    [InlineData(IccRenderingIntent.RelativeColorimetric, "Rel")]
    [InlineData(IccRenderingIntent.Saturation, "Sat")]
    [InlineData(IccRenderingIntent.AbsoluteColorimetric, "Abs")]
    public void RenderingIntentChangesDescriptionAndHeaderWithoutChangingTransforms(IccRenderingIntent intent, string name)
    {
        foreach (bool grayscale in new[] { false, true })
            foreach (bool gamma in new[] { false, true })
            {
                byte[] profile = Create(grayscale, gamma, intent);
                Dictionary<string, Tag> tags = ParseProfile(profile, grayscale ? "GRAY" : "RGB ", intent);
                string description = Text(profile, tags["desc"]);
                Assert.Equal((grayscale ? "Gra_0.34567;0.3585_" : "RGB_0.34567;0.3585_0.67;0.31;0.21;0.71;0.14;0.08_")
                    + name + "_" + (gamma ? "g0.454545" : "SRG"), description);
                byte[] perceptual = Create(grayscale, gamma, IccRenderingIntent.Perceptual);
                Dictionary<string, Tag> expected = ParseProfile(perceptual, grayscale ? "GRAY" : "RGB ", IccRenderingIntent.Perceptual);
                Assert.Equal(tags.Keys, expected.Keys);
                foreach ((string signature, Tag tag) in tags)
                    if (signature != "desc")
                        Assert.Equal(perceptual.AsSpan(expected[signature].Offset, expected[signature].Length).ToArray(),
                            profile.AsSpan(tag.Offset, tag.Length).ToArray());
            }
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.2)]
    [InlineData(2.60000000001)]
    [InlineData(8192.0)]
    public void CustomGammaFactoriesUseTheDecodingExponentAndSharedTypeZeroCurve(double gamma)
    {
        foreach (bool grayscale in new[] { false, true })
        {
            byte[] profile = grayscale
                ? JpegliIccProfiles.CreateGammaGrayscaleProfile(CustomWhite, gamma)
                : JpegliIccProfiles.CreateGammaRgbProfile(CustomPrimaries, CustomWhite, gamma);
            Dictionary<string, Tag> tags = ParseProfile(profile, grayscale ? "GRAY" : "RGB ", IccRenderingIntent.Perceptual);
            Tag curve = tags[grayscale ? "kTRC" : "rTRC"];
            Assert.Equal("para", Signature(profile, curve.Offset));
            Assert.Equal(16, curve.Length);
            Assert.Equal(0, UInt16(profile, curve.Offset + 8));
            Close(gamma, Fixed(profile, curve.Offset + 12), 0.00002, "gamma exponent");
            Assert.DoesNotContain("cicp", tags.Keys);
            if (!grayscale)
            {
                Assert.Equal(curve, tags["gTRC"]);
                Assert.Equal(curve, tags["bTRC"]);
            }
            foreach (double value in new[] { 0, 1.0 / 255, 0.25, 0.5, 0.75, 0.9999, 1 })
                Close(Math.Pow(value, gamma), EvaluateCurve(profile, curve, value), 0.000006, "gamma decoding");
        }
    }

    [Fact]
    public void AllFactoriesReturnFreshDeterministicMutableStorageWithCorrectProfileIds()
    {
        foreach (bool grayscale in new[] { false, true })
            foreach (bool gamma in new[] { false, true })
            {
                byte[] first = Create(grayscale, gamma), second = Create(grayscale, gamma);
                Assert.NotSame(first, second);
                Assert.Equal(first, second);
                Array.Fill(first, (byte)0x5b);
                Assert.Equal(second, Create(grayscale, gamma));
                AssertProfileId(second);
            }
    }

    [Theory]
    [InlineData(double.NaN, 0.3)]
    [InlineData(double.PositiveInfinity, 0.3)]
    [InlineData(double.NegativeInfinity, 0.3)]
    [InlineData(0.3, double.NaN)]
    [InlineData(0.3, double.PositiveInfinity)]
    [InlineData(0.3, double.NegativeInfinity)]
    [InlineData(-0.0001, 0.3)]
    [InlineData(0.3, -0.0001)]
    [InlineData(0.3, 0)]
    [InlineData(1.001, 0.01)]
    [InlineData(0.3, 1.00001)]
    public void FactoriesRejectInvalidWhitesAndNonfinitePrimaryCoordinates(double x, double y)
    {
        IccChromaticity invalid = new(x, y);
        Assert.Equal("whitePoint", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateRgbProfile(CustomPrimaries, invalid, IccTransferFunction.Srgb)).ParamName);
        Assert.Equal("whitePoint", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateGammaRgbProfile(CustomPrimaries, invalid, 2.2)).ParamName);
        Assert.Equal("whitePoint", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateGrayscaleProfile(invalid, IccTransferFunction.Srgb)).ParamName);
        Assert.Equal("whitePoint", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateGammaGrayscaleProfile(invalid, 2.2)).ParamName);
        if (!double.IsFinite(x) || !double.IsFinite(y))
            foreach (IccRgbPrimaries primaries in new[] { CustomPrimaries with { Red = invalid },
                CustomPrimaries with { Green = invalid }, CustomPrimaries with { Blue = invalid } })
            {
                Assert.Equal("primaries", Assert.Throws<ArgumentOutOfRangeException>(() =>
                    JpegliIccProfiles.CreateRgbProfile(primaries, CustomWhite, IccTransferFunction.Linear)).ParamName);
                Assert.Equal("primaries", Assert.Throws<ArgumentOutOfRangeException>(() =>
                    JpegliIccProfiles.CreateGammaRgbProfile(primaries, CustomWhite, 2.2)).ParamName);
            }
    }

    [Fact]
    public void SingularOrUnrepresentableColorEncodingsCannotCreateWrappedIccValues()
    {
        foreach (IccRgbPrimaries invalid in new[]
        {
            CustomPrimaries with { Green = CustomPrimaries.Red },
            new IccRgbPrimaries(new(0.4, 0.3), new(0.4, 0.3), new(0.4, 0.3)),
            // Distinct doubles collapse to identical primaries in upstream float arithmetic.
            CustomPrimaries with { Green = new(CustomPrimaries.Red.X + 1e-12, CustomPrimaries.Red.Y + 1e-12) },
        })
        {
            Assert.Equal("primaries", Assert.Throws<ArgumentException>(() =>
                JpegliIccProfiles.CreateRgbProfile(invalid, D65, IccTransferFunction.Linear)).ParamName);
            Assert.Equal("primaries", Assert.Throws<ArgumentException>(() =>
                JpegliIccProfiles.CreateGammaRgbProfile(invalid, D65, 2.2)).ParamName);
        }
        // A thin triangle produces colorants beyond the ICC signed fixed-point range.
        IccRgbPrimaries overflow = new(new(0.6, 0.3), new(0.600001, 0.300001), new(0.15, 0.06));
        Assert.Equal("primaries", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateRgbProfile(overflow, D65, IccTransferFunction.Linear)).ParamName);
        Assert.Equal("primaries", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateGammaRgbProfile(overflow, D65, 2.2)).ParamName);
        IccRgbPrimaries floatOverflow = CustomPrimaries with { Red = new(double.MaxValue, 0.3) };
        Assert.Equal("primaries", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateRgbProfile(floatOverflow, D65, IccTransferFunction.Linear)).ParamName);
        IccChromaticity enormousWhite = new(0.3, 1e-10);
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliIccProfiles.CreateGrayscaleProfile(enormousWhite, IccTransferFunction.Linear));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliIccProfiles.CreateGammaGrayscaleProfile(enormousWhite, 2.2));
        IccChromaticity underflowWhite = new(0.3, double.Epsilon);
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliIccProfiles.CreateRgbProfile(CustomPrimaries, underflowWhite, IccTransferFunction.Linear));
        Assert.Throws<ArgumentOutOfRangeException>(() => JpegliIccProfiles.CreateGammaRgbProfile(CustomPrimaries, underflowWhite, 2.2));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(0.999999999)]
    [InlineData(0.0)]
    [InlineData(-2.2)]
    [InlineData(8192.000000001)]
    public void GammaValidationUsesTheOriginalDoubleBeforeFloatRounding(double gamma)
    {
        Assert.Equal("gamma", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateGammaRgbProfile(CustomPrimaries, CustomWhite, gamma)).ParamName);
        Assert.Equal("gamma", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JpegliIccProfiles.CreateGammaGrayscaleProfile(CustomWhite, gamma)).ParamName);
    }

    [Fact]
    public void CustomFactoriesRejectInvalidTransferFunctionsAndRenderingIntents()
    {
        foreach (int value in new[] { -1, 0, 2, 255, int.MaxValue })
        {
            Assert.Equal("transferFunction", Assert.Throws<ArgumentOutOfRangeException>(() =>
                JpegliIccProfiles.CreateRgbProfile(CustomPrimaries, CustomWhite, (IccTransferFunction)value)).ParamName);
            Assert.Equal("transferFunction", Assert.Throws<ArgumentOutOfRangeException>(() =>
                JpegliIccProfiles.CreateGrayscaleProfile(CustomWhite, (IccTransferFunction)value)).ParamName);
        }
        foreach (int value in new[] { -1, 4, 255, int.MaxValue })
        {
            IccRenderingIntent intent = (IccRenderingIntent)value;
            Assert.Equal("renderingIntent", Assert.Throws<ArgumentOutOfRangeException>(() =>
                JpegliIccProfiles.CreateRgbProfile(CustomPrimaries, CustomWhite, IccTransferFunction.Linear, intent)).ParamName);
            Assert.Equal("renderingIntent", Assert.Throws<ArgumentOutOfRangeException>(() =>
                JpegliIccProfiles.CreateGrayscaleProfile(CustomWhite, IccTransferFunction.Linear, intent)).ParamName);
            Assert.Equal("renderingIntent", Assert.Throws<ArgumentOutOfRangeException>(() =>
                JpegliIccProfiles.CreateGammaRgbProfile(CustomPrimaries, CustomWhite, 2.2, intent)).ParamName);
            Assert.Equal("renderingIntent", Assert.Throws<ArgumentOutOfRangeException>(() =>
                JpegliIccProfiles.CreateGammaGrayscaleProfile(CustomWhite, 2.2, intent)).ParamName);
        }
    }

    [Theory]
    [InlineData(0, JpegColorSpace.Rgb, false, 0)]
    [InlineData(1, JpegColorSpace.Rgb, true, 0)]
    [InlineData(2, JpegColorSpace.YCbCr, true, 2)]
    [InlineData(0, JpegColorSpace.Grayscale, true, 2)]
    [InlineData(1, JpegColorSpace.Grayscale, false, 0)]
    [InlineData(2, JpegColorSpace.Grayscale, true, 0)]
    public void EmbeddingPreservesExactApp2AndCodingAcrossPrecisionsOutputsScansAndStreams(
        int kind, JpegColorSpace colorSpace, bool progressive, int bits)
    {
        const int width = 19, height = 17;
        bool gray = colorSpace == JpegColorSpace.Grayscale;
        int stride = width * (gray ? 1 : 4) + 5;
        Array source = Source(kind, width, height, stride, gray);
        Array original = (Array)source.Clone();
        byte[] profile = kind == 1 ? Create(gray, true)
            : gray ? JpegliIccProfiles.CreateGrayscaleProfile(CustomWhite, IccTransferFunction.Hlg)
            : JpegliIccProfiles.CreateRgbProfile(CustomPrimaries, CustomWhite, IccTransferFunction.Pq);
        byte[] backing = Enumerable.Repeat((byte)0x5b, profile.Length + 11).ToArray();
        profile.CopyTo(backing, 3);
        byte[] originalBacking = backing.ToArray();
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, IccProfile = backing.AsMemory(3, profile.Length),
            Distance = 1.375f, SmoothingFactor = 37, Progressive = progressive,
            ProgressiveRefinementBits = bits, RestartInterval = 2, OptimizeHuffmanTables = progressive
        };
        byte[] actual = Encode(source, width, height, stride, gray, options);
        (byte[]? icc, byte[] coding) = ReadIccAndCoding(actual);
        Assert.Equal(profile, icc);
        Assert.Equal(coding, ReadIccAndCoding(Encode(source, width, height, stride, gray,
            options with { IccProfile = new byte[] { 1, 2, 3, 4 } })).Coding);
        var withoutProfile = ReadIccAndCoding(Encode(source, width, height, stride, gray, options with { IccProfile = default }));
        Assert.Null(withoutProfile.Icc);
        Assert.Equal(coding, withoutProfile.Coding);
        Assert.Equal(actual, Encode(source, width, height, stride, gray, options with { InferCicpTransferFunctionFromIccProfile = true }));
        JpegDocument document = JpegDocument.Read(actual, progressive, 2, bits);
        Assert.Equal((width, height), (document.Width, document.Height));
        Assert.Equal(gray ? 1 : 3, document.Components.Count);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        Encode(output, source, width, height, stride, gray, options);
        Assert.Equal(actual, destination.ToArray());
        Assert.False(output.Closed);
        Assert.Equal(originalBacking, backing);
        Assert.Equal(original.Cast<object>(), source.Cast<object>());
    }

    private static byte[] Create(bool grayscale, bool gamma,
        IccRenderingIntent intent = IccRenderingIntent.Perceptual) => (grayscale, gamma) switch
    {
        (true, true) => JpegliIccProfiles.CreateGammaGrayscaleProfile(CustomWhite, 2.2, intent),
        (true, false) => JpegliIccProfiles.CreateGrayscaleProfile(CustomWhite, IccTransferFunction.Srgb, intent),
        (false, true) => JpegliIccProfiles.CreateGammaRgbProfile(CustomPrimaries, CustomWhite, 2.2, intent),
        _ => JpegliIccProfiles.CreateRgbProfile(CustomPrimaries, CustomWhite, IccTransferFunction.Srgb, intent)
    };

    private static Dictionary<string, Tag> ParseProfile(byte[] profile, string colorSpace, IccRenderingIntent intent)
    {
        Assert.Equal(profile.Length, Int32(profile, 0));
        Assert.Equal("jxl ", Signature(profile, 4));
        Assert.Equal(0x04400000, Int32(profile, 8));
        Assert.Equal("mntr", Signature(profile, 12));
        Assert.Equal(colorSpace, Signature(profile, 16));
        Assert.Equal("XYZ ", Signature(profile, 20));
        Assert.Equal(new[] { 2019, 12, 1, 0, 0, 0 }, Enumerable.Range(0, 6).Select(index => UInt16(profile, 24 + index * 2)));
        Assert.Equal("acsp", Signature(profile, 36));
        Assert.Equal("APPL", Signature(profile, 40));
        Assert.All(profile[44..64], value => Assert.Equal(0, value));
        Assert.Equal((int)intent, Int32(profile, 64));
        Assert.Equal(new[] { 0xf6d6, 0x10000, 0xd32d }, Enumerable.Range(0, 3).Select(index => Int32(profile, 68 + index * 4)));
        Assert.Equal("jxl ", Signature(profile, 80));
        Assert.All(profile[100..128], value => Assert.Equal(0, value));
        AssertProfileId(profile);

        int count = Int32(profile, 128), end = 132 + count * 12;
        var tags = new Dictionary<string, Tag>();
        var extents = new HashSet<Tag>();
        for (int index = 0; index < count; index++)
        {
            int offset = 132 + index * 12;
            var tag = new Tag(Int32(profile, offset + 4), Int32(profile, offset + 8));
            Assert.True(tags.TryAdd(Signature(profile, offset), tag));
            Assert.Equal(0, tag.Offset % 4);
            Assert.InRange(tag.Offset, end, profile.Length - 8);
            Assert.InRange(tag.Length, 8, profile.Length - tag.Offset);
            Assert.Equal(0, Int32(profile, tag.Offset + 4));
            extents.Add(tag);
        }
        Tag[] ordered = extents.OrderBy(tag => tag.Offset).ToArray();
        Assert.Equal(end, ordered[0].Offset);
        for (int index = 1; index < ordered.Length; index++)
        {
            int previousEnd = ordered[index - 1].Offset + ordered[index - 1].Length;
            Assert.Equal((previousEnd + 3) & ~3, ordered[index].Offset);
            Assert.All(profile[previousEnd..ordered[index].Offset], value => Assert.Equal(0, value));
        }
        Assert.Equal(profile.Length, ordered[^1].Offset + ordered[^1].Length);
        Assert.Equal("CC0", Text(profile, tags["cprt"]));
        Assert.NotEmpty(Text(profile, tags["desc"]));
        return tags;
    }

    private static void AssertProfileId(byte[] profile)
    {
        byte[] hashed = profile.ToArray();
        Array.Clear(hashed, 44, 4);
        Array.Clear(hashed, 64, 4);
        Array.Clear(hashed, 84, 16);
        Assert.Equal(profile[84..100], MD5.HashData(hashed));
    }

    private static string Text(byte[] profile, Tag tag)
    {
        Assert.Equal("mluc", Signature(profile, tag.Offset));
        Assert.Equal(1, Int32(profile, tag.Offset + 8));
        Assert.Equal(12, Int32(profile, tag.Offset + 12));
        Assert.Equal("enUS", Signature(profile, tag.Offset + 16));
        int length = Int32(profile, tag.Offset + 20), offset = Int32(profile, tag.Offset + 24);
        Assert.Equal(28, offset);
        Assert.Equal(0, length % 2);
        Assert.InRange(length, 0, tag.Length - offset);
        Assert.All(profile[(tag.Offset + offset + length)..(tag.Offset + tag.Length)], value => Assert.Equal(0, value));
        return Encoding.BigEndianUnicode.GetString(profile, tag.Offset + offset, length);
    }

    private static void VerifyCurve(byte[] profile, Tag tag, IccTransferFunction transfer)
    {
        if (transfer is IccTransferFunction.Pq or IccTransferFunction.Hlg)
        {
            Assert.Equal("curv", Signature(profile, tag.Offset));
            Assert.Equal(140, tag.Length);
            Assert.Equal(64, Int32(profile, tag.Offset + 8));
            int previous = -1;
            for (int index = 0; index < 64; index++)
            {
                int entry = UInt16(profile, tag.Offset + 12 + index * 2);
                Assert.InRange(entry, previous, 65535);
                Close(Decode(transfer, index / 63.0), entry / 65535.0, 0.00002, "HDR decoding table");
                previous = entry;
            }
            Assert.Equal(0, UInt16(profile, tag.Offset + 12));
            Assert.Equal(65535, UInt16(profile, tag.Offset + 138));
        }
        else
        {
            Assert.Equal("para", Signature(profile, tag.Offset));
            Assert.Equal(32, tag.Length);
            Assert.Equal(3, UInt16(profile, tag.Offset + 8));
            Assert.Equal(0, UInt16(profile, tag.Offset + 10));
            double threshold = Fixed(profile, tag.Offset + 28);
            foreach (double value in Enumerable.Range(0, 256).Select(index => index / 255.0)
                .Concat(new[] { -0.125, -1e-8, threshold - 1e-8, threshold, threshold + 1e-8 }))
                Close(Decode(transfer, value, threshold), EvaluateCurve(profile, tag, value), 0.00004, "parametric decoding");
        }
    }

    private static double Decode(IccTransferFunction transfer, double encoded, double? roundedThreshold = null) => transfer switch
    {
        IccTransferFunction.Linear => encoded,
        IccTransferFunction.Dci => encoded < 0 ? encoded : Math.Pow(encoded, 2.6),
        IccTransferFunction.Srgb => encoded < (roundedThreshold ?? 0.04045)
            ? encoded / 12.92 : Math.Pow((encoded + 0.055) / 1.055, 2.4),
        IccTransferFunction.Rec709 => encoded < (roundedThreshold ?? 0.081)
            ? encoded / 4.5 : Math.Pow((encoded + 0.099) / 1.099, 1 / 0.45),
        IccTransferFunction.Hlg => encoded <= 0.5 ? encoded * encoded / 3
            : (Math.Exp((encoded - 0.55991073) / 0.17883277) + 0.28466892) / 12,
        IccTransferFunction.Pq => Pq(encoded),
        _ => throw new ArgumentOutOfRangeException(nameof(transfer))
    };

    private static double Pq(double encoded)
    {
        const double m1 = 2610.0 / 16384, m2 = 2523.0 / 32;
        const double c1 = 3424.0 / 4096, c2 = 2413.0 / 128, c3 = 2392.0 / 128;
        double power = Math.Pow(encoded, 1 / m2);
        return Math.Pow(Math.Max(0, power - c1) / (c2 - c3 * power), 1 / m1);
    }

    private static double EvaluateCurve(byte[] profile, Tag tag, double encoded)
    {
        if (Signature(profile, tag.Offset) == "curv")
        {
            double position = Math.Clamp(encoded, 0, 1) * 63;
            int left = Math.Min(62, (int)position);
            double fraction = position - left;
            return ((1 - fraction) * UInt16(profile, tag.Offset + 12 + left * 2)
                + fraction * UInt16(profile, tag.Offset + 14 + left * 2)) / 65535;
        }
        double gamma = Fixed(profile, tag.Offset + 12);
        if (UInt16(profile, tag.Offset + 8) == 0)
            return Math.Pow(encoded, gamma);
        double a = Fixed(profile, tag.Offset + 16), b = Fixed(profile, tag.Offset + 20),
            c = Fixed(profile, tag.Offset + 24), threshold = Fixed(profile, tag.Offset + 28);
        return encoded < threshold ? c * encoded : Math.Pow(a * encoded + b, gamma);
    }

    private static double[,] AdaptedPrimaries(IccRgbPrimaries primaries, IccChromaticity white, double[,] adaptation)
    {
        // Independent double-precision oracle: solve for the scale factors
        // from xy coordinates, then adapt each XYZ column to analytic D50.
        IccChromaticity[] channels = [primaries.Red, primaries.Green, primaries.Blue];
        double[,] unscaled = new double[3, 3];
        for (int column = 0; column < 3; column++)
        {
            double[] xyz = [channels[column].X, channels[column].Y, 1 - channels[column].X - channels[column].Y];
            for (int row = 0; row < 3; row++)
                unscaled[row, column] = xyz[row];
        }
        double[] scale = Solve(unscaled, Xyz(white));
        double[,] result = new double[3, 3];
        for (int column = 0; column < 3; column++)
        {
            double[] xyz = Enumerable.Range(0, 3).Select(row => unscaled[row, column] * scale[column]).ToArray();
            double[] adapted = Multiply(adaptation, xyz);
            for (int row = 0; row < 3; row++)
                result[row, column] = adapted[row];
        }
        return result;
    }

    private static double[,] Bradford(IccChromaticity white)
    {
        double[,] basis = { { 0.8951, 0.2664, -0.1614 }, { -0.7502, 1.7135, 0.0367 }, { 0.0389, -0.0685, 1.0296 } };
        double[] source = Multiply(basis, Xyz(white)), destination = Multiply(basis, [0.96422, 1, 0.82521]);
        double[,] result = new double[3, 3];
        for (int column = 0; column < 3; column++)
        {
            double[] adapted = new double[3];
            for (int row = 0; row < 3; row++)
                adapted[row] = basis[row, column] * destination[row] / source[row];
            double[] xyz = Solve(basis, adapted);
            for (int row = 0; row < 3; row++)
                result[row, column] = xyz[row];
        }
        return result;
    }

    private static double[] Xyz(IccChromaticity xy) => [xy.X / xy.Y, 1, (1 - xy.X - xy.Y) / xy.Y];

    private static double[] Solve(double[,] matrix, double[] vector)
    {
        double[,] augmented = new double[3, 4];
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
                augmented[row, column] = matrix[row, column];
            augmented[row, 3] = vector[row];
        }
        for (int column = 0; column < 3; column++)
        {
            int pivot = column;
            for (int row = column + 1; row < 3; row++)
                if (Math.Abs(augmented[row, column]) > Math.Abs(augmented[pivot, column]))
                    pivot = row;
            for (int index = column; index < 4; index++)
                (augmented[column, index], augmented[pivot, index]) = (augmented[pivot, index], augmented[column, index]);
            double divisor = augmented[column, column];
            for (int index = column; index < 4; index++)
                augmented[column, index] /= divisor;
            for (int row = 0; row < 3; row++)
            {
                if (row == column)
                    continue;
                double factor = augmented[row, column];
                for (int index = column; index < 4; index++)
                    augmented[row, index] -= factor * augmented[column, index];
            }
        }
        return [augmented[0, 3], augmented[1, 3], augmented[2, 3]];
    }

    private static double[] Multiply(double[,] matrix, double[] vector)
    {
        double[] result = new double[3];
        for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
                result[row] += matrix[row, column] * vector[column];
        return result;
    }

    private static (byte[]? Icc, byte[] Coding) ReadIccAndCoding(byte[] jpeg)
    {
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        using var coding = new MemoryStream();
        coding.Write(jpeg.AsSpan(0, 2));
        byte[]? profile = null;
        int offset = 2;
        while (jpeg[offset + 1] != 0xda)
        {
            Assert.Equal(0xff, jpeg[offset]);
            int length = UInt16(jpeg, offset + 2);
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
        return (profile, coding.ToArray());
    }

    private static Array Source(int kind, int width, int height, int stride, bool gray)
    {
        int components = gray ? 1 : 4, rowLength = width * components;
        byte[] bytes = Enumerable.Repeat((byte)0xad, (height - 1) * stride + rowLength).ToArray();
        for (int row = 0; row < height; row++)
            for (int column = 0; column < width; column++)
                for (int channel = 0; channel < (gray ? 1 : 3); channel++)
                    bytes[row * stride + column * components + channel] = (byte)((column * 73 + row * 29 + channel * 61) % 256);
        return kind switch
        {
            0 => bytes,
            1 => bytes.Select((value, index) => (ushort)Math.Clamp(value * 257 + index % 47 - 23, 0, 65535)).ToArray(),
            _ => bytes.Select((value, index) => index % stride >= rowLength || !gray && index % stride % 4 == 3
                ? float.NaN : Math.Clamp(value / 255f + (index % 47 - 23) / 65535f, 0f, 1f)).ToArray()
        };
    }

    private static byte[] Encode(Array source, int width, int height, int stride, bool gray, JpegliEncoderOptions options) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, gray ? PixelLayout.Gray8 : PixelLayout.Bgra32,
            options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, gray ? PixelComponentLayout.Gray : PixelComponentLayout.Bgra,
            options, stride, TestContext.Current.CancellationToken),
        _ => JpegliEncoder.Encode((float[])source, width, height, gray ? PixelComponentLayout.Gray : PixelComponentLayout.Bgra,
            options, stride, TestContext.Current.CancellationToken)
    };

    private static void Encode(Stream output, Array source, int width, int height, int stride, bool gray, JpegliEncoderOptions options)
    {
        if (source is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, gray ? PixelLayout.Gray8 : PixelLayout.Bgra32,
                options, stride, TestContext.Current.CancellationToken);
        else if (source is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, gray ? PixelComponentLayout.Gray : PixelComponentLayout.Bgra,
                options, stride, TestContext.Current.CancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])source, width, height, gray ? PixelComponentLayout.Gray : PixelComponentLayout.Bgra,
                options, stride, TestContext.Current.CancellationToken);
    }

    private static int Int32(byte[] bytes, int offset) => BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset, 4));
    private static int UInt16(byte[] bytes, int offset) => BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(offset, 2));
    private static double Fixed(byte[] bytes, int offset) => Int32(bytes, offset) / 65536.0;
    private static string Signature(byte[] bytes, int offset) => Encoding.ASCII.GetString(bytes, offset, 4);
    private readonly record struct Tag(int Offset, int Length);
    private static void Close(double expected, double actual, double tolerance, string label) =>
        Assert.True(double.IsFinite(actual) && Math.Abs(expected - actual) <= tolerance,
            $"{label}: expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
}
