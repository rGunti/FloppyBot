using Microsoft.Extensions.Hosting;

namespace FloppyBot.Base.Configuration;

public static class RunSequence
{
    public static THost Do<THost>(this THost host, Action<THost> action)
        where THost : IHost
    {
        action(host);
        return host;
    }
}
