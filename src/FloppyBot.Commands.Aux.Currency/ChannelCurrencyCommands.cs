using System.Collections.Immutable;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Storage;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Currency.BackgroundServices;
using FloppyBot.Commands.Aux.Currency.Storage;
using FloppyBot.Commands.Aux.Currency.Storage.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Core.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Aux.Currency;

[CommandHost]
[PrivilegeGuard(PrivilegeLevel.Viewer)]
[CommandCategory("Currency")]
// ReSharper disable once UnusedType.Global
public class ChannelCurrencyCommands
{
    public const string ReplyCurrentBalance = "Your current balance is: {Balance} {CurrencyName}";
    public const string ReplyUpdatedBalance =
        "The balance has been updated to: {Balance} {CurrencyName}";
    public const string ReplyBalanceCleared = "The balance has been cleared";

    public const string ReplyCurrencyEnabled =
        "Channel Currency \"{CurrencyName}\" has been enabled";
    public const string ReplyCurrencyNameChanged =
        "Channel Currency has been renamed to \"{CurrencyName}\"";
    public const string ReplySetDefaultCurrency =
        "Default balance for new users has been set to {Balance} {CurrencyName}";
    public const string ReplyCurrencyDisabled =
        "Channel Currency has been disabled on this channel";

    public const string ReplyInvalidAmount = "Amount must be greater than 0";
    public const string ReplyInvalidAmountNotANumber = "Amount must be a valid number";
    public const string ReplyInvalidCurrencyName = "You have to provide a currency name";
    public const string ReplyInvalidCommand = "Invalid command";

    public const string ArgumentAll = "all";

    private readonly IChannelCurrencyService _channelCurrencyService;
    private readonly IChannelCurrencySettingsService _settingsService;
    private readonly IAuditor _auditor;

    public ChannelCurrencyCommands(
        IChannelCurrencyService channelCurrencyService,
        IAuditor auditor,
        IChannelCurrencySettingsService settingsService
    )
    {
        _channelCurrencyService = channelCurrencyService;
        _auditor = auditor;
        _settingsService = settingsService;
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void RegisterDependencies(IServiceCollection services)
    {
        services
            .AddScoped<IChannelCurrencyService, ChannelCurrencyService>()
            .AddScoped<IChannelCurrencySettingsService, ChannelCurrencySettingsService>()
            .AddScoped<IChannelUserStateService, ChannelUserStateService>()
            .AddAuditor<StorageAuditor>()
            .AddHostedService<ChannelCurrencyMaintainer>();
    }

    [Command("cash")]
    [PrimaryCommandName("cash")]
    [CommandDescription("Check your current balance")]
    public string? GetCurrency([SourceChannel] string sourceChannel, [Author] ChatUser author)
    {
        var settings = _settingsService.GetChannelCurrencySettings(sourceChannel);
        if (!settings.HasValue)
        {
            return null;
        }

        var record = _channelCurrencyService
            .GetChannelCurrency(sourceChannel, author.Identifier.Channel)
            .FirstOrDefault(
                new ChannelCurrencyRecord(
                    null!,
                    sourceChannel,
                    author.Identifier.Channel,
                    settings.Value.StartingBalance
                )
            );

        return ReplyCurrentBalance.Format(MessageParameters.From(record, settings));
    }

    [Command("givecash")]
    [PrimaryCommandName("givecash")]
    [CommandDescription("Give a chat user some currency")]
    [PrivilegeGuard(PrivilegeLevel.Moderator)]
    [CommandSyntax("<User> <Amount>")]
    [CommandParameterHint(1, "User", CommandParameterType.String)]
    [CommandParameterHint(2, "Amount", CommandParameterType.Number)]
    public string? GiveCurrency(
        [SourceChannel] string sourceChannel,
        [Author] ChatUser author,
        [ArgumentIndex(0)] string target,
        [ArgumentIndex(1)] string amountStr
    )
    {
        var settings = _settingsService.GetChannelCurrencySettings(sourceChannel);
        if (!settings.HasValue)
        {
            return null;
        }

        if (!int.TryParse(amountStr, out var amount) || amount <= 0)
        {
            return ReplyInvalidAmount;
        }

        var result = _channelCurrencyService.IncrementCurrency(sourceChannel, target, amount);
        _auditor.CurrencyUpdated(author, sourceChannel, result);
        return ReplyUpdatedBalance.Format(MessageParameters.From(result, settings));
    }

    [Command("takecash")]
    [PrimaryCommandName("takecash")]
    [PrivilegeGuard(PrivilegeLevel.Moderator)]
    [CommandSyntax("<User> <Amount | 'all'>")]
    [CommandParameterHint(1, "User", CommandParameterType.String)]
    [CommandParameterHint(2, "Amount", CommandParameterType.Number)]
    public string? TakeCurrency(
        [SourceChannel] string sourceChannel,
        [Author] ChatUser author,
        [ArgumentIndex(0)] string target,
        [ArgumentIndex(1)] string amountStr
    )
    {
        var settings = _settingsService.GetChannelCurrencySettings(sourceChannel);
        if (!settings.HasValue)
        {
            return null;
        }

        if (amountStr.Equals(ArgumentAll, StringComparison.InvariantCultureIgnoreCase))
        {
            _channelCurrencyService.ClearBalance(sourceChannel, target);
            _auditor.CurrencyDeleted(author, sourceChannel);
            return ReplyBalanceCleared;
        }

        if (!int.TryParse(amountStr, out int amount))
        {
            return ReplyInvalidAmountNotANumber;
        }

        if (amount <= 0)
        {
            return ReplyInvalidAmount;
        }

        var result = _channelCurrencyService.IncrementCurrency(sourceChannel, target, -amount);
        _auditor.CurrencyUpdated(author, sourceChannel, result);
        return ReplyUpdatedBalance.Format(MessageParameters.From(result, settings));
    }

    [Command("cashconfig")]
    [PrimaryCommandName("cashconfig")]
    [PrivilegeGuard(PrivilegeLevel.Moderator)]
    [CommandSyntax(
        "<enable | disable | setname | set-name | setbalance | set-balance> <Arguments>"
    )]
    public string? ConfigureChannelCurrency(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)] string subCommand,
        [ArgumentRange(1, stopIfMissing: false, outputAsArray: true)]
            IImmutableList<string> arguments
    )
    {
        switch (subCommand)
        {
            case "enable":
                return EnableCurrency(sourceChannel);
            case "disable":
                return DisableCurrency(sourceChannel);
            case "setname":
            case "set-name":
                return RenameCurrency(sourceChannel, string.Join(" ", arguments));
            case "setbalance":
            case "set-balance":
                return SetDefaultBalance(sourceChannel, arguments[0]);
            default:
                return ReplyInvalidCommand;
        }
    }

    private string? RenameCurrency(string sourceChannel, string currencyName)
    {
        if (string.IsNullOrWhiteSpace(currencyName))
        {
            return ReplyInvalidCurrencyName;
        }

        var settings = _settingsService
            .GetChannelCurrencySettings(sourceChannel)
            .FirstOrDefault(ChannelCurrencySettings.GetDefault(sourceChannel)) with
        {
            CurrencyName = currencyName,
        };

        _settingsService.SetChannelCurrencySettings(settings);
        return ReplyCurrencyNameChanged.Format(MessageParameters.FromSettings(settings));
    }

    private string? SetDefaultBalance(string sourceChannel, string argument)
    {
        if (!int.TryParse(argument, out var balance) || balance < 0)
        {
            return ReplyInvalidAmount;
        }

        var settings = _settingsService
            .GetChannelCurrencySettings(sourceChannel)
            .FirstOrDefault(ChannelCurrencySettings.GetDefault(sourceChannel)) with
        {
            StartingBalance = balance,
        };

        _settingsService.SetChannelCurrencySettings(settings);
        return ReplySetDefaultCurrency.Format(MessageParameters.FromSettings(settings));
    }

    private string? EnableCurrency(string sourceChannel)
    {
        var settings = _settingsService
            .GetChannelCurrencySettings(sourceChannel)
            .Select(s => s with { Enabled = true })
            .FirstOrDefault();
        if (settings is null)
        {
            return ReplyInvalidCurrencyName;
        }

        _settingsService.SetChannelCurrencySettings(settings);
        return ReplyCurrencyEnabled.Format(MessageParameters.FromSettings(settings));
    }

    private string? DisableCurrency(string sourceChannel)
    {
        _settingsService.DisableChannelCurrencySettings(sourceChannel);
        return ReplyCurrencyDisabled;
    }

    private record MessageParameters(string CurrencyName, int Balance)
    {
        public static MessageParameters FromSettings(ChannelCurrencySettings settings)
        {
            return new MessageParameters(settings.CurrencyName, settings.StartingBalance);
        }

        public static MessageParameters From(
            ChannelCurrencyRecord record,
            ChannelCurrencySettings settings
        )
        {
            return new MessageParameters(settings.CurrencyName, record.Balance);
        }
    }
}
