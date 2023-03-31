using Microsoft.AspNetCore.Mvc;

namespace serilog_webapi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{

    [HttpGet()]
    public async Task<IActionResult> Get()
    {
        return Ok();
    }
}