using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Middleman.Util;
using Proxy;
using static Proxy.Greeter;

namespace Middleman.Controllers;

[ApiController]
[Route("[controller]/")]
public class GrpcController : ControllerBase, IDisposable
{
    private IWebHostEnvironment _env;
    private GrpcChannel channel;
    private GreeterClient client;


    public GrpcController(IWebHostEnvironment env)
    {
        _env = env;
        channel = GrpcChannel.ForAddress("https://localhost:5002/grpc");
        client = new GreeterClient(channel);
    }

    [HttpGet("send-receive")]
    async public Task<IActionResult> SendReceiveOverHttps()
    {
        await client.FooAsync(new HelloRequest { Name = Payload.CurrentPayload });
        return Ok($"Done, payload: {Payload.CurrentPayload.Length} bytes");
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
