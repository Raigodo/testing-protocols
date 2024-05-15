using Grpc.Net.Client;
using System.Net.WebSockets;

namespace GatheringMetrics.Util;

public static class SetupHelper
{
    public static ClientWebSocket Ws { get; }
    public static HttpClient Http { get; }
    public static GrpcChannel Grpc { get; }

    //executed when class acessed for the first time
    static SetupHelper()
    {
        Ws = new ClientWebSocket();
        Http = new HttpClient();
        Grpc = GrpcChannel.ForAddress("https://localhost:5002/grpc");
    }

    async public static Task StartWsConnection()
    {
        await Ws.ConnectAsync(new Uri("wss://localhost:5002/ws"), CancellationToken.None);
    }

    async public static Task CloseWsConnection()
    {
        await Ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, default, CancellationToken.None);
    }
}
