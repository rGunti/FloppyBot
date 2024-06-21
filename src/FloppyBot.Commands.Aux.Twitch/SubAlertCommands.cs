using System.Collections.Immutable;
using FloppyBot.Aux.TwitchAlerts.Core;
using FloppyBot.Aux.TwitchAlerts.Core.Entities;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Aux.Twitch;

[CommandHost]
[SourceInterfaceGuard("Twitch")]
[PrivilegeGuard(PrivilegeLevel.Moderator)]
[CommandCategory("Community")]
public class SubAlertCommands
{
    private const string REPLY_ALERT_SET = "✅ Sub Alert Message has been set";
    private const string REPLY_ALERT_CLEAR = "✅ Sub Alert Message has been cleared";

    private const string CHAT_MESSAGE_FORMAT =
        "{User} just subscribed with {SubscriptionTier}! Thank you so much for the support! 🎉";

    private readonly ITwitchAlertService _alertService;
    private readonly IAuditor _auditor;

    public SubAlertCommands(ITwitchAlertService alertService, IAuditor auditor)
    {
        _alertService = alertService;
        _auditor = auditor;
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void RegisterDependencies(IServiceCollection services)
    {
        services.AddTwitchAlertCore();
    }

    [Command("subalert")]
    [CommandDescription("Sets the message to be sent when someone subscribes to the channel.")]
    [CommandSyntax("<Sub Alert Message>")]
    // ReSharper disable once UnusedMember.Global
    public string SetAlertMessage(
        [Author] ChatUser author,
        [SourceChannel] string sourceChannel,
        [AllArguments] string message
    )
    {
        var settings =
            _alertService.GetAlertSettings(sourceChannel)
            ?? new TwitchAlertSettings { Id = sourceChannel };

        settings = settings with
        {
            SubMessage = ImmutableList
                .Create<TwitchAlertMessage>()
                .Add(new TwitchAlertMessage(message)),
            SubAlertsEnabled = true,
        };

        _alertService.StoreAlertSettings(settings);
        _auditor.SubAlertMessageSet(author, sourceChannel, message);
        return REPLY_ALERT_SET;
    }

    [Command("clearalert")]
    [CommandDescription("Clears the message to be sent when someone subscribes to the channel.")]
    [CommandSyntax("")]
    // ReSharper disable once UnusedMember.Global
    public string ClearAlertMessage([Author] ChatUser author, [SourceChannel] string sourceChannel)
    {
        var settings = _alertService.GetAlertSettings(sourceChannel);
        if (settings is not null)
        {
            settings = settings with { SubAlertsEnabled = false, };
            _alertService.StoreAlertSettings(settings);
            _auditor.SubAlertMessageDisabled(author, sourceChannel);
        }

        return REPLY_ALERT_CLEAR;
    }

    [Command("setdefaultalert")]
    [CommandDescription("Resets the sub alert message to the default message")]
    [CommandSyntax("")]
    // ReSharper disable once UnusedMember.Global
    public string SetDefaultAlertMessage(
        [Author] ChatUser author,
        [SourceChannel] string sourceChannel
    )
    {
        var settings =
            _alertService.GetAlertSettings(sourceChannel)
            ?? new TwitchAlertSettings { Id = sourceChannel };

        settings = settings with
        {
            SubMessage = ImmutableList
                .Create<TwitchAlertMessage>()
                .Add(new TwitchAlertMessage(CHAT_MESSAGE_FORMAT)),
            SubAlertsEnabled = true,
        };

        _alertService.StoreAlertSettings(settings);
        _auditor.SubAlertMessageCleared(author, sourceChannel);
        return REPLY_ALERT_SET;
    }
}
