using Grpc.Net.Client;
using System.Net;
using System.Net.WebSockets;

namespace GatheringMetrics.Util.Callers;

public class ControlledCall : CallBase, ICaller
{
    protected ClientWebSocket _wsClient { get; private set; }
    protected HttpClient _http20Client { get; private set; }
    protected HttpClient _http30Client { get; private set; }
    protected GrpcChannel _grpcChannel { get; private set; }
    protected Greeter.GreeterClient _grpcClient { get; private set; }

    //executed when class acessed for the first time
    protected ControlledCall() { }

    public static async Task<ControlledCall> Create()
    {
        var caller = new ControlledCall();

        caller._wsClient = await caller.MakeWsClientAsync();

        caller._http20Client = caller.MakeHttpClient(HttpVersion.Version20);
        caller._http30Client = caller.MakeHttpClient(HttpVersion.Version30);


        var grpcChannel = caller.MakeGrpcChannel();
        caller._grpcChannel = grpcChannel;
        caller._grpcClient = caller.MakeGrpcClient(grpcChannel);

        return caller;
    }


    public async Task MakeCallOverHttp20Async() => await TestCallOverHttp20Async(_http20Client);
    public async Task MakeCallOverHttp30Async() => await TestCallOverHttp30Async(_http30Client);
    public async Task MakeCallOverWsAsync() => await TestCallOverWsAsync(_wsClient);
    public async Task MakeCallOverGrpcAsync() => await TestCallOverGrpcAsync(_grpcClient);

    public async Task CleanUp()
    {
        if (_wsClient.State == WebSocketState.Open)
            await _wsClient.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, default, CancellationToken.None);
        _wsClient.Dispose();
        _http20Client.Dispose();
        _http30Client.Dispose();
        _grpcChannel.Dispose();
        _grpcChannel.Dispose();
    }

    public async Task EnsureCleanedUp()
    {
        await CleanUp();
    }
}
