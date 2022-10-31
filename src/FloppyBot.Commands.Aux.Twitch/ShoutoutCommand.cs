using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Twitch.Api;
using FloppyBot.Commands.Aux.Twitch.Config;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Guards;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Api.Interfaces;

namespace FloppyBot.Commands.Aux.Twitch;

[CommandHost]
[SourceInterfaceGuard("Twitch")]
// ReSharper disable once UnusedType.Global
public class ShoutoutCommand
{
    // TODO: When database system is in place, this needs to be replaced
    private const string REPLY =
        "!!! SHOUTOUT TO {AccountName} !!! They last played {LastGame}, which was no doubt a lot of fun. Check here: {Link}";

    private readonly ITwitchApiService _twitchApiService;

    public ShoutoutCommand(ITwitchApiService twitchApiService)
    {
        _twitchApiService = twitchApiService;
    }

    [Command("shoutout", "so")]
    [PrivilegeGuard(PrivilegeLevel.Moderator)]
    // ReSharper disable once UnusedMember.Global
    public async Task<string?> Shoutout(
        [ArgumentIndex(0)] string channel)
    {
        var query = await _twitchApiService.LookupUser(channel);
        if (query == null)
        {
            return null;
        }

        return REPLY.Format(query);
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void RegisterDependencies(IServiceCollection services)
    {
        services
            .AddSingleton<TwitchApiConfig>(s => s
                .GetRequiredService<IConfiguration>()
                .GetSection("TwitchApi")
                .Get<TwitchApiConfig>())
            .AddSingleton<ITwitchAPI>(s =>
            {
                var config = s.GetRequiredService<TwitchApiConfig>();
                var api = new TwitchAPI(s.GetRequiredService<ILoggerFactory>())
                {
                    Settings =
                    {
                        ClientId = config.ClientId,
                        Secret = config.Secret
                    }
                };
                return api;
            })
            .AddScoped<ITwitchApiService, TwitchApiService>();
    }
}
