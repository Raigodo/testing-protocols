using Microsoft.AspNetCore.Mvc;
using Middleman.Util;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace Middleman.Controllers;

[ApiController]
[Route("[controller]/")]
public class WebSocketController : ControllerBase
{
    static ClientWebSocket _ws;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);


    [HttpPost("connect")]
    async public Task<IActionResult> Connect()
    {
        await _semaphore.WaitAsync();

        try
        {
            if (_ws is not null || _ws?.State == WebSocketState.Open)
                return BadRequest("Already connected");

            _ws = new();
            await _ws!.ConnectAsync(new Uri($"wss://localhost:5002/ws"), CancellationToken.None);
            Console.WriteLine("WS connection created");

        }
        finally { _semaphore.Release(); }
        return Ok("success");
    }

    [HttpPost("disconnect")]
    async public Task<IActionResult> Disconnect()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_ws is null || _ws!.State != WebSocketState.Open)
                return BadRequest("No active websocket connection");

            await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, default, CancellationToken.None);
            _ws.Dispose();
            _ws = null;
            Console.WriteLine("WS connection disposed");
        }
        finally { _semaphore.Release(); }
        return Ok("success");
    }

    [HttpGet("send-receive")]
    async public Task<IActionResult> SendReceive(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync();
        try
        {
            var message = Payload.CurrentPayload;
            var bytes = Encoding.UTF8.GetBytes(message);
            var requestSegment = new ArraySegment<byte>(bytes);

            await _ws.SendAsync(requestSegment, WebSocketMessageType.Text, true, CancellationToken.None);

            var buffer = new byte[1024];
            var responseSegment = new ArraySegment<byte>(buffer);
            var response = await _ws.ReceiveAsync(responseSegment, CancellationToken.None);
            message = Encoding.UTF8.GetString(buffer, 0, response.Count);

            return Ok($"Done, payload: {message.Length} bytes");
        }
        finally { _semaphore.Release(); }
    }
}