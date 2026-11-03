using System.Diagnostics;
using System.Numerics;
using DotJpegli.Performance;

// One-session local benchmark exception explicitly granted by the user.
// The committed performance CLI retains its normal CI guard.
internal static class ResearchStartup
{
    public static async Task<int> Main(string[] args)
    {
        using Process process = Process.GetCurrentProcess();
        long available = (long)process.ProcessorAffinity;
        process.ProcessorAffinity = (nint)(1L << BitOperations.Log2((ulong)available));
        process.PriorityClass = ProcessPriorityClass.AboveNormal;
        Console.WriteLine($"User-authorized local research; affinity={(long)process.ProcessorAffinity}; priority={process.PriorityClass}; tiering disabled.");
        return await AcReciprocalCommand.Run(args.Skip(1).ToArray());
    }
}
