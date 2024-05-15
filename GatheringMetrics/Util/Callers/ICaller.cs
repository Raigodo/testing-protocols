namespace GatheringMetrics.Util.Callers;

public interface ICaller
{
    public Task MakeCallOverHttp20Async();
    public Task MakeCallOverHttp30Async();
    public Task MakeCallOverWsAsync();
    public Task MakeCallOverGrpcAsync();

    public Task EnsureCleanedUp();
}
