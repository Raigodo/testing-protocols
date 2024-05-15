using GatheringMetrics.Util.Callers;

namespace GatheringMetrics.Util.Gather;

public class MetricsGatherer(ICaller caller) : IDisposable
{
    public void Dispose()
    {
        caller.EnsureCleanedUp();
    }

    public async Task GatherMemoryUsageAsync(int iterations)
    {
        Console.WriteLine("\nMemory allocated [bytes]");
        Console.WriteLine($"HTTP/2:        {await CallAnalyzer.GetMemoryAllocated(caller.MakeCallOverHttp20Async, iterations)}");
        Console.WriteLine($"HTTP/3:        {await CallAnalyzer.GetMemoryAllocated(caller.MakeCallOverHttp30Async, iterations)}");
        Console.WriteLine($"WS:            {await CallAnalyzer.GetMemoryAllocated(caller.MakeCallOverWsAsync, iterations)}");
        Console.WriteLine($"gRPC:          {await CallAnalyzer.GetMemoryAllocated(caller.MakeCallOverGrpcAsync, iterations)}");
    }

    public async Task GatherWaitTimeAsync(int iterations)
    {
        Console.WriteLine("\nWait time [ticks]");
        Console.WriteLine($"HTTP/2:        {await CallAnalyzer.GetReponseTime(caller.MakeCallOverHttp20Async, iterations)}");
        Console.WriteLine($"HTTP/3:        {await CallAnalyzer.GetReponseTime(caller.MakeCallOverHttp30Async, iterations)}");
        Console.WriteLine($"WS:            {await CallAnalyzer.GetReponseTime(caller.MakeCallOverWsAsync, iterations)}");
        Console.WriteLine($"gRPC:          {await CallAnalyzer.GetReponseTime(caller.MakeCallOverGrpcAsync, iterations)}");
    }
}
