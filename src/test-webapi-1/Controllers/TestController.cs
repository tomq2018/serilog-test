using Microsoft.AspNetCore.Mvc;

namespace test_webapi_1.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{

    [HttpGet("user")]
    public async Task<IActionResult> GetUser([FromQuery] string user)
    {
        return Ok(user);
    }
}