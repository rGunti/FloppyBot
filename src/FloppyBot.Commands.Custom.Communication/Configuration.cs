using FloppyBot.Base.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FloppyBot.Commands.Custom.Communication;

public static class Configuration
{
    public const string SOUND_CMD = "SoundCommandInvocation";

    public static string GetSoundCommandInvocationConfigString(this IConfiguration configuration)
    {
        return configuration.GetParsedConnectionString(SOUND_CMD);
    }

    public static THost StartSoundCommandInvocationReceiver<THost>(this THost host)
        where THost : IHost
    {
        host.Services.GetRequiredService<ISoundCommandInvocationReceiver>();
        return host;
    }
}
