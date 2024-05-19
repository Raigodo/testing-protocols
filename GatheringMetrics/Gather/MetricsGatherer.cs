using GatheringMetrics.Callers;
using GatheringMetrics.Values.Enums;

namespace GatheringMetrics.Gather;

public class MetricsGatherer(ICaller caller) : IDisposable
{
    public void Dispose()
    {
        caller.Dispose();
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
        int waitSeconds = 10;
        return new()
        {
            { Protocols.HTTP20, await CallAnalyzer.CountCalls(caller.MakeCallOverHttp20Async, waitSeconds, targetProtocolName: "http2")},
            { Protocols.HTTP30, await CallAnalyzer.CountCalls(caller.MakeCallOverHttp30Async, waitSeconds, targetProtocolName: "http3")},
            { Protocols.GRPC,   await CallAnalyzer.CountCalls(caller.MakeCallOverGrpcAsync,   waitSeconds, targetProtocolName: "grpc")},
            { Protocols.WS,     await CallAnalyzer.CountCalls(caller.MakeCallOverWsAsync,     waitSeconds, targetProtocolName: "ws")},
        };
    }


}
