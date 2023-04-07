using Microsoft.AspNetCore.Mvc;

namespace serilog_webapi.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly TestApi _testApi;
    private readonly ILogger<TestController> _logger;

    public TestController(TestApi testApi,
        ILogger<TestController> logger)
    {
        _testApi = testApi;
        _logger = logger;
    }

    [HttpGet()]
    public async Task<IActionResult> Get()
    {
        // return Ok();
        return Ok(await _testApi.Get("tom"));
    }

    [HttpGet("test")]
    public async Task<IActionResult> GetOk()
    {
        _logger.LogInformation("test");
        return Ok();
    }
}