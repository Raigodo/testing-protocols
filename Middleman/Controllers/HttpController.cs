using Microsoft.AspNetCore.Mvc;
using Middleman.Util;
using System.Net;

namespace Middleman.Controllers;

[ApiController]
[Route("[controller]/")]
public class HttpController : ControllerBase
{
    private IWebHostEnvironment _env;

    public HttpController(IWebHostEnvironment env)
    {
        _env = env;
    }


    [HttpGet("v20/send-receive")]
    async public Task<IActionResult> SendReceiveOverHttp20()
    {
        await MakeHttpCall(Payload.CurrentPayload, HttpVersion.Version20);
        return Ok($"Done, payload: {Payload.CurrentPayload.Length} bytes");
    }




    [HttpGet("v30/send-receive")]
    async public Task<IActionResult> SendReceiveOverHttp30()
    {
        await MakeHttpCall(Payload.CurrentPayload, HttpVersion.Version30);
        return Ok($"Done, payload: {Payload.CurrentPayload.Length} bytes");
    }


    async private Task<string> MakeHttpCall(string payload, Version version)
    {
        using var client = new HttpClient();

        using var request = new HttpRequestMessage
        {
            RequestUri = new Uri("https://localhost:5002/http"),
            Version = version,
            Method = HttpMethod.Post,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact,
            Content = JsonContent.Create(payload),
        };

        using HttpResponseMessage response = await client.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }
}
