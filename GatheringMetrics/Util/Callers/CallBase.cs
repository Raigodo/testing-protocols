using Grpc.Net.Client;
using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;

namespace GatheringMetrics.Util;

public abstract class CallBase
{
    private readonly int PORT = 5002;
    protected async Task<ClientWebSocket> MakeWsClientAsync()
    {
        var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri($"wss://localhost:{PORT}/ws"), CancellationToken.None);
        return ws;
    }
    protected async Task DisposeWsClientAsync(ClientWebSocket ws)
    {
        await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, default, CancellationToken.None);
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
        var message = Payload.CurrentPayload;
        var bytes = Encoding.UTF8.GetBytes(message);
        var arraySegment = new ArraySegment<byte>(bytes);
        await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[1024];
        await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
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
