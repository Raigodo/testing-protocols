using Microservice;
using Microservice.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseKestrel()
    .UseQuic()
    .ConfigureKestrel((ctx, opt) =>
    {
        opt.ConfigureEndpointDefaults(listenOptions =>
        {
            listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3;
            listenOptions.UseHttps();
        });
    });


builder.Services.AddGrpc();
builder.Services.AddEndpointsApiExplorer();



var app = builder.Build();


ThreadPool.SetMinThreads(workerThreads: 100, completionPortThreads: 100);


app.UseWebSockets();

app.UseHttpsRedirection();


app.MapGrpcService<GreeterService>();
app.MapPost("/grpc", (HelloRequest request) => request.Name);


app.MapPost("/http", async (HttpContext http, [FromBody] string payload) =>
{
    //Console.WriteLine(payload);
    await http.Response.WriteAsync($"used {http.Request.Protocol} -> {payload}");
});

app.MapGet("/ws", async (HttpContext context, CancellationToken cancellationToken) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        //WebSocketAcceptContext acceptOptions = new()
        //{
        //    DangerousEnableCompression = false,
        //};
        using var ws = await context.WebSockets.AcceptWebSocketAsync();

        await HandleWebSocketAsync(ws);
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

app.Run();


async Task HandleWebSocketAsync(WebSocket ws)
{
    var buffer = new byte[1024 * 4];
    var receiveSegment = new ArraySegment<byte>(buffer);

    while (ws.State == WebSocketState.Open)
    {
        var request = await ws.ReceiveAsync(receiveSegment, CancellationToken.None);

        if (ws.State != WebSocketState.Open)
        {
            await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            break;
        }

        if (request.MessageType == WebSocketMessageType.Text && request.EndOfMessage)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, request.Count);
            var responseBytes = Encoding.UTF8.GetBytes(message);
            var responseSegment = new ArraySegment<byte>(responseBytes);

            await ws.SendAsync(
                responseSegment,
                WebSocketMessageType.Text,
                WebSocketMessageFlags.EndOfMessage,
                CancellationToken.None);
        }
        
    }
}