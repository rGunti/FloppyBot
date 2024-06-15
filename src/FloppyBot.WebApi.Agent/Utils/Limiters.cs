using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace FloppyBot.WebApi.Agent.Utils;

internal static class Limiters
{
    private const string KEY_GLOBAL = "global";
    private const string KEY_AUTH = "auth";

    private const string LOGGER_CATEGORY = "RateLimiter";

    private const string SECTION_DEFAULT = "RateLimiter:Default";
    private const string SECTION_AUTH = "RateLimiter:Authenticated";

    internal static IServiceCollection ConfigureRateLimiter(
        this IServiceCollection services,
        IConfiguration config
    )
    {
        return services
            .Configure<TokenBucketRateLimiterOptions>(
                KEY_GLOBAL,
                o => config.GetRequiredSection(SECTION_DEFAULT).Bind(o)
            )
            .Configure<TokenBucketRateLimiterOptions>(
                KEY_AUTH,
                o => config.GetRequiredSection(SECTION_AUTH).Bind(o)
            )
            .AddRateLimiter(rl =>
            {
                rl.OnRejected = RateLimiter_OnRejected;
                rl.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                rl.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    RateLimiter_Build
                );
            });
    }

    private static RateLimitPartition<string> RateLimiter_Build(HttpContext httpContext)
    {
        var logger = httpContext.GetLogger(LOGGER_CATEGORY);

        string? accessToken = httpContext.Request.Headers.Authorization.ToString().HashString();
        string? remoteIp = httpContext.GetRemoteHostIpFromHeaders()?.ToString();

        var partitionKey = accessToken ?? remoteIp ?? KEY_GLOBAL;
        logger.LogTrace("Building rate limiter for partition={PartitionKey}", partitionKey);
        return RateLimitPartition.GetTokenBucketLimiter(
            accessToken ?? remoteIp ?? KEY_GLOBAL,
            _ =>
                httpContext
                    .RequestServices.GetRequiredService<
                        IOptionsFactory<TokenBucketRateLimiterOptions>
                    >()
                    .Create(!string.IsNullOrWhiteSpace(accessToken) ? KEY_AUTH : KEY_GLOBAL)
        );
    }

    private static async ValueTask RateLimiter_OnRejected(
        OnRejectedContext context,
        CancellationToken cancellationToken
    )
    {
        var response = context.HttpContext.Response;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            // Add a Retry-After header to the response
            response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(
                NumberFormatInfo.InvariantInfo
            );
        }

        response.StatusCode = StatusCodes.Status429TooManyRequests;

        context
            .HttpContext.GetLogger(LOGGER_CATEGORY)
            .LogWarning(
                "Request rejected from addr={UserRequestAddress} to path={UserRequestPath}",
                context.HttpContext.GetRemoteHostIpFromHeaders(),
                context.HttpContext.Request.Path
            );
        await response.WriteAsync(
            "Whoo there, calm down, mate! You're exceeding the speed limit here, gonna call the cops next time.",
            cancellationToken
        );
    }

    private static string? HashString(this string? s)
    {
        return string.IsNullOrWhiteSpace(s)
            ? null
            : Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
    }
}
