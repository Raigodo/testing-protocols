using GatheringMetrics.Util;
using Grpc.Net.Client;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;

namespace GatheringMetrics.Callers;

public abstract class CallBase
{
    protected const int PORT = 5002;
    private static readonly SemaphoreSlim _wsSemaphore = new SemaphoreSlim(1, 1);
    protected async Task<ClientWebSocket> MakeWsClientAsync()
    {
        await _wsSemaphore.WaitAsync();
        var ws = new ClientWebSocket();
        try
        {
            await ws.ConnectAsync(new Uri($"wss://localhost:{PORT}/ws?bs={Payload.CurrentPayload.Length}"), CancellationToken.None);
        }
        finally { _wsSemaphore.Release(); }
        return ws;
    }
    protected async Task DisposeWsClientAsync(ClientWebSocket ws)
    {
        await _wsSemaphore.WaitAsync();
        if (ws is null || ws!.State != WebSocketState.Open)
            return;

        try
        {
            await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, default, CancellationToken.None);
            ws.Dispose();
        }
        finally { _wsSemaphore.Release(); }
        ws.Dispose();
    }

    protected HttpClient MakeHttpClient(Version version)
    {
        var http = new HttpClient()
        {
            BaseAddress = new Uri($"https://localhost:{PORT}/http"),
            DefaultRequestVersion = version,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };
        return http;
    }

    protected GrpcChannel MakeGrpcChannel() =>
        GrpcChannel.ForAddress($"https://localhost:{PORT}/grpc");

    protected Greeter.GreeterClient MakeGrpcClient(GrpcChannel channel) =>
        new Greeter.GreeterClient(channel);





    protected async Task TestCallOverWsAsync(ClientWebSocket ws)
    {
        await _wsSemaphore.WaitAsync();
        try
        {
            var message = Payload.CurrentPayload;
            var bytes = Encoding.UTF8.GetBytes(message);
            var requestSegment = new ArraySegment<byte>(bytes);

            await ws.SendAsync(requestSegment, WebSocketMessageType.Text, true, CancellationToken.None);

            var buffer = new byte[Payload.CurrentPayload.Length];
            var responseSegment = new ArraySegment<byte>(buffer);
            var response = await ws.ReceiveAsync(responseSegment, CancellationToken.None);
            message = Encoding.UTF8.GetString(buffer, 0, response.Count);
        }
        finally { _wsSemaphore.Release(); }
    }

    protected async Task TestCallOverHttp20Async(HttpClient client)
    {
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = JsonContent.Create(Payload.CurrentPayload),
        };

        await PerformHttpCallAsync(request, client);
    }

    protected async Task TestCallOverHttp30Async(HttpClient client)
    {
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = JsonContent.Create(Payload.CurrentPayload),
        };

        await PerformHttpCallAsync(request, client);
    }

    protected async Task TestCallOverGrpcAsync(Greeter.GreeterClient client) =>
        await client.FooAsync(new HelloRequest { Name = Payload.CurrentPayload });

    protected async Task PerformHttpCallAsync(HttpRequestMessage request, HttpClient client) =>
        await client.SendAsync(request);
}
