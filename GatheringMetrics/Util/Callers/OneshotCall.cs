using System.Net;

namespace GatheringMetrics.Util.Callers;

public class OneshotCall : CallBase, ICaller
{
    public async Task MakeCallOverHttp20Async()
    {
        using var client = MakeHttpClient(HttpVersion.Version20);
        await TestCallOverHttp20Async(client);
    }

    public async Task MakeCallOverHttp30Async()
    {
        using var client = MakeHttpClient(HttpVersion.Version30);
        await TestCallOverHttp30Async(client);
    }
    public async Task MakeCallOverWsAsync()
    {
        using var ws = await MakeWsClientAsync();
        await TestCallOverWsAsync(ws);
        await DisposeWsClientAsync(ws);
    }

    public async Task MakeCallOverGrpcAsync()
    {
        using var grpcChannel = MakeGrpcChannel();
        var client = MakeGrpcClient(grpcChannel);
        await TestCallOverGrpcAsync(client);
    }

    public Task EnsureCleanedUp() => Task.CompletedTask;
}
