using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FloppyBot.Base.Cron;

public static class HostingExtensions
{
    public static THost BootCronJobs<THost>(this THost host)
        where THost : IHost
    {
        host.Services.GetRequiredService<ICronManager>().Launch();
        return host;
    }
}
