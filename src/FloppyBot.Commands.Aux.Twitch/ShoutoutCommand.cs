using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Twitch.Api;
using FloppyBot.Commands.Aux.Twitch.Config;
using FloppyBot.Commands.Aux.Twitch.Storage;
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
[PrivilegeGuard(PrivilegeLevel.Moderator)]
// ReSharper disable once UnusedType.Global
public class ShoutoutCommand
{
    private readonly IShoutoutMessageSettingService _shoutoutMessageSettingService;
    private readonly ITwitchApiService _twitchApiService;

    public ShoutoutCommand(
        ITwitchApiService twitchApiService,
        IShoutoutMessageSettingService shoutoutMessageSettingService)
    {
        _twitchApiService = twitchApiService;
        _shoutoutMessageSettingService = shoutoutMessageSettingService;
    }

    [Command("shoutout", "so")]
    // ReSharper disable once UnusedMember.Global
    public async Task<string?> Shoutout(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)] string channel)
    {
        var setting = _shoutoutMessageSettingService.GetSettings(sourceChannel);
        if (setting == null || string.IsNullOrEmpty(setting.Message))
        {
            return null;
        }

        var query = await _twitchApiService.LookupUser(channel);
        if (query == null)
        {
            return null;
        }

        return setting.Message.Format(query);
    }

    [Command("setshoutout")]
    // ReSharper disable once UnusedMember.Global
    public string SetShoutout(
        [SourceChannel] string sourceChannel,
        [AllArguments] string template)
    {
        _shoutoutMessageSettingService.SetShoutoutMessage(sourceChannel, template);
        return "✅ Shoutout Message has been set";
    }

    [Command("clearshoutout")]
    // ReSharper disable once UnusedMember.Global
    public string ClearShoutout([SourceChannel] string sourceChannel)
    {
        _shoutoutMessageSettingService.ClearSettings(sourceChannel);
        return "✅ Shoutout Message has been cleared";
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
            .AddScoped<ITwitchApiService, TwitchApiService>()
            .AddScoped<IShoutoutMessageSettingService, ShoutoutMessageSettingService>();
    }
}
