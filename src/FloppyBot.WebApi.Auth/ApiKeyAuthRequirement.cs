using Microsoft.AspNetCore.Authorization;

namespace FloppyBot.WebApi.Auth;

public record ApiKeyAuthRequirement : IAuthorizationRequirement
{
    private const string HEADER_API_KEY = "X-Api-Key";
    private const string QUERY_API_KEY = "apiKey";

    public static ApiKeyAuthRequirement Instance { get; } = new();

    private ApiKeyAuthRequirement()
    {
        HeaderName = HEADER_API_KEY;
        QueryName = QUERY_API_KEY;
    }

    public string HeaderName { get; }
    public string QueryName { get; }
}
