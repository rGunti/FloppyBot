using FloppyBot.Version;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Base.Logging;

public static class LoggedStartup
{
    public static Task LogAndRun(
        this IHost host)
    {
        host.Services.GetRequiredService<ILogger<Run>>()
            .LogInformation(
                "Starting up application {AppName} {ServiceName} {ServiceVersion}",
                AboutThisApp.Name,
                AboutThisApp.ServiceName,
                AboutThisApp.Version);
        return host
            .RunAsync();
    }

    private class Run
    {
    }
}
