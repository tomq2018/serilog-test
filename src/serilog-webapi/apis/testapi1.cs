using Refit;

public interface TestApi
{
    [Get("/test/user")]
    Task<string> Get(string user);
}