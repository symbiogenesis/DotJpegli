namespace DotJpegli.Automation;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            if (args.Length == 0 || args is ["--help"])
            {
                Console.WriteLine("DotJpegli automation. Use the scripts in tools/ for supported command entry points.");
                return 0;
            }

            var options = new Arguments(args[1..]);
            switch (args[0])
            {
                case "infer-sharp":
                    options.Validate(["report", "translation-log", "baseline", "framework", "source-root"], ["self-test"]);
                    InferSharpCommand.Run(options);
                    break;
                case "corpus":
                    options.Validate(["output", "source", "manifest"], ["self-test"]);
                    await CorpusCommand.Run(options);
                    break;
                default:
                    throw new ArgumentException($"Unknown command: {args[0]}");
            }

            return 0;
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or InvalidOperationException or System.Text.Json.JsonException or System.Xml.XmlException or System.Net.Http.HttpRequestException or OperationCanceledException)
        {
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}
