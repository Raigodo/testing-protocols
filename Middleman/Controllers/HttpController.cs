using Microsoft.AspNetCore.Mvc;
using Middleman.Util;
using System.Net;

namespace Middleman.Controllers;

[ApiController]
[Route("[controller]/")]
public class HttpController : ControllerBase
{
    private IWebHostEnvironment _env;

    static HttpClient http20Client = new HttpClient()
    {
        BaseAddress = new Uri($"https://localhost:5002/http"),
        DefaultRequestVersion = HttpVersion.Version20,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
    };
    static HttpClient http30Client = new HttpClient()
    {
        BaseAddress = new Uri($"https://localhost:5002/http"),
        DefaultRequestVersion = HttpVersion.Version30,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact,
    };

    public HttpController(IWebHostEnvironment env)
    {
        _env = env;
    }


    [HttpGet("v20/send-receive")]
    async public Task<IActionResult> SendReceiveOverHttp20()
    {
        await MakeHttpCall(Payload.CurrentPayload, http20Client);
        return Ok($"Done, payload: {Payload.CurrentPayload.Length} bytes");
    }




    [HttpGet("v30/send-receive")]
    async public Task<IActionResult> SendReceiveOverHttp30()
    {
        await MakeHttpCall(Payload.CurrentPayload, http30Client);
        return Ok($"Done, payload: {Payload.CurrentPayload.Length} bytes");
    }


    async private Task<string> MakeHttpCall(string payload, HttpClient client)
    {
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = JsonContent.Create(payload),
        };

        using HttpResponseMessage response = await client.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }
}
