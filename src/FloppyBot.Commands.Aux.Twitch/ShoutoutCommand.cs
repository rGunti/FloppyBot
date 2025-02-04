using FloppyBot.Aux.MessageCounter.Core;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Cron;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Aux.Twitch.Api;
using FloppyBot.Commands.Aux.Twitch.Config;
using FloppyBot.Commands.Aux.Twitch.Helpers;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartFormat.Core.Formatting;
using TwitchLib.Api;
using TwitchLib.Api.Interfaces;

namespace FloppyBot.Commands.Aux.Twitch;

[CommandHost]
[SourceInterfaceGuard("Twitch")]
[PrivilegeGuard(PrivilegeLevel.Moderator)]
[CommandCategory("Community")]
// ReSharper disable once UnusedType.Global
public class ShoutoutCommand
{
    public const string REPLY_SAVE = "✅ Shoutout Message has been set";
    public const string REPLY_CLEAR = "✅ Shoutout Message has been cleared";

    public const string REPLY_ERR_FORMATTING =
        "🛑 Failed to format your shoutout message. Make sure all placeholders are spelt correctly.";

    private readonly ILogger<ShoutoutCommand> _logger;

    private readonly IShoutoutMessageSettingService _shoutoutMessageSettingService;
    private readonly ITwitchApiService _twitchApiService;
    private readonly IAuditor _auditor;

    public ShoutoutCommand(
        ITwitchApiService twitchApiService,
        IShoutoutMessageSettingService shoutoutMessageSettingService,
        ILogger<ShoutoutCommand> logger,
        IAuditor auditor
    )
    {
        _twitchApiService = twitchApiService;
        _shoutoutMessageSettingService = shoutoutMessageSettingService;
        _logger = logger;
        _auditor = auditor;
    }

    [DependencyRegistration]
    public static void RegisterDependencies(IServiceCollection services)
    {
        services
            .AddSingleton<TwitchApiConfig>(s =>
                s.GetRequiredService<IConfiguration>()
                    .GetSection("TwitchApi")
                    .Get<TwitchApiConfig>()!
            )
            .AddSingleton<ITwitchAPI>(s =>
            {
                var config = s.GetRequiredService<TwitchApiConfig>();
                var api = new TwitchAPI(s.GetRequiredService<ILoggerFactory>())
                {
                    Settings = { ClientId = config.ClientId, Secret = config.Secret },
                };
                return api;
            })
            .AddScoped<ITwitchApiService, TwitchApiService>()
            .AddScoped<IShoutoutMessageSettingService, ShoutoutMessageSettingService>()
            .AddMemoryCache();
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void SetupTimerMessageDependencies(IServiceCollection services)
    {
        SetupTimerMessageDbDependencies(services);
        services.AddCronJob<TimerMessageCronJob>();
    }

    public static void SetupTimerMessageDbDependencies(IServiceCollection services)
    {
        services
            .AddMessageOccurrenceService()
            .AddTransient<ITimerMessageConfigurationService, TimerMessageConfigurationService>();
    }

    [Command("shoutout", "so")]
    [CommandDescription(
        "Shouts out a Twitch channel with a customized message defined for the channel"
    )]
    [CommandSyntax("<Channel Name>", "Avinnus", "pinsrltrex")]
    // ReSharper disable once UnusedMember.Global
    public async Task<string?> Shoutout(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)] string channel
    )
    {
        var setting = _shoutoutMessageSettingService.GetSettings(sourceChannel);
        if (setting == null || string.IsNullOrEmpty(setting.Message))
        {
            return null;
        }

        var userLookup = await _twitchApiService.LookupUser(channel);
        if (userLookup == null)
        {
            return null;
        }

        string messageTemplate = setting.Message;
        TwitchStreamTeamResult? raiderTeamLookup = null;

        if (!string.IsNullOrWhiteSpace(setting.TeamMessage))
        {
            var userTeamLookup = await _twitchApiService.LookupTeam(
                sourceChannel.ParseAsChannelId().Channel
            );
            raiderTeamLookup = await _twitchApiService.LookupTeam(channel);

            if (
                userTeamLookup is not null
                && raiderTeamLookup is not null
                && userTeamLookup.IsSameTeam(raiderTeamLookup)
            )
            {
                messageTemplate = setting.TeamMessage;
            }
        }
        else
        {
            _logger.LogTrace(
                "No team message configured for {Channel}, skipping lookup",
                sourceChannel
            );
        }

        try
        {
            return messageTemplate.Format(
                MessageTemplateParams.FromLookups(userLookup, raiderTeamLookup)
            );
        }
        catch (FormattingException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to format message. Message template was {MessageTemplate}",
                setting.Message
            );
            return REPLY_ERR_FORMATTING;
        }
    }

    [Command("setshoutout", "setso")]
    [CommandDescription(
        "Sets the shoutout template for the requesting channel. "
            + "The following placeholders are supported, when surrounded by {}: "
            + $"{nameof(MessageTemplateParams.AccountName)}, "
            + $"{nameof(MessageTemplateParams.DisplayName)}, "
            + $"{nameof(MessageTemplateParams.LastGame)}, "
            + $"{nameof(MessageTemplateParams.Link)}"
    )]
    [CommandSyntax(
        "<Message>",
        "Shoutout to {DisplayName} at {Link}. They last played {LastGame}!"
    )]
    // ReSharper disable once UnusedMember.Global
    public string SetShoutout(
        [Author] ChatUser author,
        [SourceChannel] ChannelIdentifier sourceChannel,
        [AllArguments] string template
    )
    {
        _shoutoutMessageSettingService.SetShoutoutMessage(sourceChannel, template);
        _auditor.ShoutoutMessageSet(author, sourceChannel, template);
        return REPLY_SAVE;
    }

    [Command("setteamshoutout", "setteamso")]
    [CommandDescription(
        "Sets the shoutout template for the requesting channel when the raiding channel is part of the same team. "
            + "The following placeholders are supported, when surrounded by {}: "
            + $"{nameof(MessageTemplateParams.AccountName)}, "
            + $"{nameof(MessageTemplateParams.DisplayName)}, "
            + $"{nameof(MessageTemplateParams.LastGame)}, "
            + $"{nameof(MessageTemplateParams.Link)}, "
            + $"{nameof(MessageTemplateParams.TeamId)}, "
            + $"{nameof(MessageTemplateParams.TeamSlug)}, "
            + $"{nameof(MessageTemplateParams.TeamName)}, "
            + $"{nameof(MessageTemplateParams.TeamLink)}"
    )]
    public string? SetTeamShoutout(
        [Author] ChatUser author,
        [SourceChannel] ChannelIdentifier sourceChannel,
        [AllArguments] string template
    )
    {
        var settings =
            _shoutoutMessageSettingService.GetSettings(sourceChannel)
            ?? new ShoutoutMessageSetting(sourceChannel, template, null);

        _shoutoutMessageSettingService.SetShoutoutMessage(settings with { TeamMessage = template });
        _auditor.TeamShoutoutMessageSet(author, sourceChannel, template);
        return REPLY_SAVE;
    }

    [Command("clearshoutout", "clearso")]
    [CommandDescription(
        "Clears the channels shoutout message, effectively disabling the shoutout command"
    )]
    // ReSharper disable once UnusedMember.Global
    public string ClearShoutout(
        [Author] ChatUser author,
        [SourceChannel] ChannelIdentifier sourceChannel
    )
    {
        _shoutoutMessageSettingService.ClearSettings(sourceChannel);
        _auditor.ShoutoutMessageCleared(author, sourceChannel);
        return REPLY_CLEAR;
    }
}
