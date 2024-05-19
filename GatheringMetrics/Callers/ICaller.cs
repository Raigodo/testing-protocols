namespace GatheringMetrics.Callers;

public interface ICaller : IDisposable
{
    public Task MakeCallOverHttp20Async();
    public Task MakeCallOverHttp30Async();
    public Task MakeCallOverWsAsync();
    public Task MakeCallOverGrpcAsync();
}
