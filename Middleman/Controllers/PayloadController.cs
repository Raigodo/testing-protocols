using Microsoft.AspNetCore.Mvc;
using Middleman.Util;

namespace Middleman.Controllers;


[ApiController]
[Route("[controller]/")]
public class PayloadController : ControllerBase
{
    [HttpPatch("current/{fileName}")]
    async public Task<IActionResult> SetUsedPayloadFromFile(string fileName)
    {
        await Payload.PreparePayload(fileName);
        return Ok("success");
    }

    [HttpGet("current")]
    public IActionResult SetUsedPayloadFromFile()
    {
        return Ok(Payload.CurrentPayload.Length);
    }


}
