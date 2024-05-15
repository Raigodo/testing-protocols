using Microservice;
using Microservice.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.WebSockets;
using System.Text;

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


app.UseWebSockets();

app.UseHttpsRedirection();


app.MapGrpcService<GreeterService>();
app.MapPost("/grpc", (HelloRequest request) => request.Name);


app.MapPost("/http", async (HttpContext http, [FromBody] string payload) =>
{
    //Console.WriteLine(payload);
    await http.Response.WriteAsync($"used {http.Request.Protocol} -> {payload}");
});

app.MapGet("/ws", async (HttpContext context) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        while (true)
        {
            if (ws.State != WebSocketState.Open)
                break;
            var buffer = new byte[1024];
            await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var request = Encoding.UTF8.GetString(buffer);

            var message = "Response from microservice from ws";
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes);
            if (ws.State != WebSocketState.Open)
                break;
            await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

app.Run();