using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Middleman.Util;
using Proxy;

namespace Middleman.Controllers;

[ApiController]
[Route("[controller]/")]
public class GrpcController : ControllerBase
{
    private IWebHostEnvironment _env;

    public GrpcController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("send-receive")]
    async public Task<IActionResult> SendReceiveOverHttps()
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:5002/grpc");
        var client = new Greeter.GreeterClient(channel);
        var reply = await client.FooAsync(new HelloRequest { Name = Payload.CurrentPayload });
        return Ok($"Done, payload: {Payload.CurrentPayload.Length} bytes");
    }
}
