using GatheringMetrics.Util.Callers;
using GatheringMetrics.Util.Enums;
using System.Collections.Generic;
using System.Diagnostics;

namespace GatheringMetrics.Util.Gather;

public class MetricsGatherer(ICaller caller) : IDisposable
{
    public void Dispose()
    {
        caller.EnsureCleanedUp();
    }

    public async Task<Dictionary<Protocols, double>> GatherMemoryUsageAsync(int iterations)
    {
        return new()
        {
            { Protocols.HTTP20, await CallAnalyzer.GetAvgMemoryAllocated(caller.MakeCallOverHttp20Async, iterations) },
            { Protocols.HTTP30, await CallAnalyzer.GetAvgMemoryAllocated(caller.MakeCallOverHttp30Async, iterations) },
            { Protocols.WS, await CallAnalyzer.GetAvgMemoryAllocated(caller.MakeCallOverWsAsync, iterations) },
            { Protocols.GRPC, await CallAnalyzer.GetAvgMemoryAllocated(caller.MakeCallOverGrpcAsync, iterations) },
        };
    }

    public async Task<Dictionary<Protocols, double>> GatherWaitTimeAsync(int iterations)
    {
        return new()
        {
            { Protocols.HTTP20, await CallAnalyzer.GetAvgReponseTime(caller.MakeCallOverHttp20Async, iterations) },
            { Protocols.HTTP30, await CallAnalyzer.GetAvgReponseTime(caller.MakeCallOverHttp30Async, iterations) },
            { Protocols.WS, await CallAnalyzer.GetAvgReponseTime(caller.MakeCallOverWsAsync, iterations) },
            { Protocols.GRPC, await CallAnalyzer.GetAvgReponseTime(caller.MakeCallOverGrpcAsync, iterations) },
        };
    }

    public async Task<Dictionary<Protocols, double>> GatherCpuLoad(int iterations)
    {
        return new()
        {
            { Protocols.HTTP20, await CallAnalyzer.GetAvgCpuUsage(caller.MakeCallOverHttp20Async, iterations) },
            { Protocols.HTTP30, await CallAnalyzer.GetAvgCpuUsage(caller.MakeCallOverHttp30Async, iterations) },
            { Protocols.WS, await CallAnalyzer.GetAvgCpuUsage(caller.MakeCallOverWsAsync, iterations) },
            { Protocols.GRPC, await CallAnalyzer.GetAvgCpuUsage(caller.MakeCallOverGrpcAsync, iterations) },
        };
    }

    public async Task<Dictionary<Protocols, double>> GatherThroughput()
    {
        int waitSeconds = 1;
        return new()
        {
            { Protocols.HTTP20, await CountCalls(caller.MakeCallOverHttp20Async, waitSeconds, targetProtocolName: "http2")},
            { Protocols.HTTP30, await CountCalls(caller.MakeCallOverHttp30Async, waitSeconds, targetProtocolName: "http3")},
            { Protocols.GRPC,   await CountCalls(caller.MakeCallOverGrpcAsync,   waitSeconds, targetProtocolName: "grpc")},
            { Protocols.WS,     await CountCalls(caller.MakeCallOverWsAsync,     waitSeconds, targetProtocolName: "ws")},
        };
    }

    private async Task<int> CountCalls(Func<Task> call, int waitSeconds = 10, string targetProtocolName = "Not specified")
    {
        Console.WriteLine($"Starting counting calls for: {targetProtocolName}");
        var cts = new CancellationTokenSource();
        var count = 0;
        var job = Task.Run(async () => count = await CallAnalyzer.GatherThroughput(call, cts.Token));

        for (var i = 0; i< waitSeconds; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            Console.Write(".");
        }
        Console.Write("\n");

        cts.Cancel();

        Console.WriteLine($"Finishing counting calls for: {targetProtocolName}");

        await job;

        return count;
    }
}
