using System.Net;
using System.Net.Sockets;

namespace FloppyBot.WebApi.Agent.Utils;

public static class HttpContextHelpers
{
    private const string HEADER_FORWARDED_FOR = "X-Forwarded-For";
    private const string HEADER_REAL_IP = "X-Real-IP";

    public static IPAddress? GetRemoteHostIpFromHeaders(this HttpContext httpContext)
    {
        return httpContext
            .Request.Headers.GetCommaSeparatedValues(HEADER_REAL_IP)
            .Concat(httpContext.Request.Headers.GetCommaSeparatedValues(HEADER_FORWARDED_FOR))
            .Select(ip => IPAddress.TryParse(ip, out var address) ? address : null)
            .FirstOrDefault(
                ip =>
                    ip?.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6,
                httpContext.Connection.RemoteIpAddress
            );
    }

    public static ILogger GetLogger(this HttpContext httpContext, string categoryName)
    {
        return httpContext
            .RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger(categoryName);
    }
}
