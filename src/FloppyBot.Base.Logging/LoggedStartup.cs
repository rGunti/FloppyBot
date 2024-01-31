using FloppyBot.Version;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Base.Logging;

public static class LoggedStartup
{
    public static Task LogAndRun(this IHost host)
    {
        var appInfo = AboutThisApp.Info;
        host.Services.GetRequiredService<ILogger<Run>>()
            .LogInformation(
                "Starting up application {AppName} {ServiceName} {ServiceVersion}",
                appInfo.Name,
                appInfo.ServiceName,
                appInfo.Version
            );
        return host.RunAsync();
    }

    private class Run { }
}
