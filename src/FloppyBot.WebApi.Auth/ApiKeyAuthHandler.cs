using System.Security.Claims;
using FloppyBot.WebApi.Auth.UserProfiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace FloppyBot.WebApi.Auth;

public class ApiKeyAuthHandler : AuthorizationHandler<ApiKeyAuthRequirement>
{
    private readonly IApiKeyService _apiKeyService;

    public ApiKeyAuthHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApiKeyAuthRequirement authRequirement
    )
    {
        var httpRequest = (context.Resource as DefaultHttpContext)?.Request;
        if (httpRequest is null)
        {
            return Task.CompletedTask;
        }

        string apiKey;
        if (HasApiKeyHeader(httpRequest, authRequirement))
        {
            apiKey = httpRequest.Headers[authRequirement.HeaderName]!;
        }
        else if (HasApiKeyQuery(httpRequest, authRequirement))
        {
            apiKey = httpRequest.Query[authRequirement.QueryName]!;
        }
        else
        {
            // Ignore
            return Task.CompletedTask;
        }

        var apiKeyDetails = _apiKeyService.GetApiKey(apiKey);
        if (apiKeyDetails is null)
        {
            // "Invalid API key."
            context.Fail(new AuthorizationFailureReason(this, "Invalid API Key provided"));
            return Task.CompletedTask;
        }

        context.Succeed(authRequirement);
        context
            .User.Identities.First()
            .AddClaim(
                new Claim(AuthExtensions.API_KEY_CHANNEL_CLAIM, apiKeyDetails.OwnedByChannelId)
            );
        return Task.CompletedTask;
    }

    private bool HasApiKeyHeader(HttpRequest request, ApiKeyAuthRequirement authRequirement)
    {
        return request.Headers.ContainsKey(authRequirement.HeaderName);
    }

    private bool HasApiKeyQuery(HttpRequest request, ApiKeyAuthRequirement authRequirement)
    {
        return request.Query.ContainsKey(authRequirement.QueryName);
    }
}
