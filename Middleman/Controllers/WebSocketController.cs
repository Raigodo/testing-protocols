using Microsoft.AspNetCore.Mvc;
using Middleman.Util;
using System.Net.WebSockets;
using System.Text;

namespace Middleman.Controllers;

[ApiController]
[Route("[controller]/")]
public class WebSocketController : ControllerBase
{
    static ClientWebSocket _ws = new ClientWebSocket();
    private IWebHostEnvironment _env;

    public WebSocketController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("connect")]
    async public Task<IActionResult> Connect()
    {
        await _ws.ConnectAsync(new Uri("wss://localhost:5002/ws"), CancellationToken.None);
        return Ok("success");
    }

    [HttpPost("disconnect")]
    async public Task<IActionResult> Disconnect()
    {
        await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, default, CancellationToken.None);
        return Ok("success");
    }


    [HttpGet("send-receive")]
    async public Task<IActionResult> SendReceive()
    {
        var message = Payload.CurrentPayload;
        var bytes = Encoding.UTF8.GetBytes(message);
        var arraySegment = new ArraySegment<byte>(bytes);
        await _ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

        var buffer = new byte[1024];
        await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var response = Encoding.UTF8.GetString(buffer);
        return Ok($"Done, payload: {Payload.CurrentPayload.Length} bytes");
    }
}
