using Microsoft.AspNetCore.Mvc;

namespace serilog_webapi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly TestApi _testApi;

    public TestController(TestApi testApi)
    {
        _testApi = testApi;
    }

    [HttpGet()]
    public async Task<IActionResult> Get()
    {
        // return Ok();
        return Ok(await _testApi.Get("tom"));
    }
}