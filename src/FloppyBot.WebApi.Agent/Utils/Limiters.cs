using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace FloppyBot.WebApi.Agent.Utils;

internal static class Limiters
{
    private const string KEY_GLOBAL = "global";
    private const string KEY_AUTH = "auth";

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
                rl.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    string? accessToken = httpContext.Request.Headers.Authorization;
                    return RateLimitPartition.GetTokenBucketLimiter(
                        accessToken ?? KEY_GLOBAL,
                        key =>
                            httpContext
                                .RequestServices.GetRequiredService<
                                    IOptionsFactory<TokenBucketRateLimiterOptions>
                                >()
                                .Create(key == KEY_GLOBAL ? KEY_GLOBAL : KEY_AUTH)
                    );
                });
            });
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
            .HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiter")
            .LogWarning(
                "Request rejected from addr={UserRequestAddress} to path={UserRequestPath}",
                context.HttpContext.Connection.RemoteIpAddress,
                context.HttpContext.Request.Path
            );
        await response.WriteAsync(
            "Whoo there, calm down, mate! You're exceeding the speed limit here, gonna call the cops next time.",
            cancellationToken
        );
    }
}
