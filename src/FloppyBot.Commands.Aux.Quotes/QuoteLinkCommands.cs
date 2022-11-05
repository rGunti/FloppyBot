using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Guards;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Quotes;

[CommandHost]
[PrivilegeGuard(PrivilegeLevel.Moderator)]
public class QuoteLinkCommands
{
    private const string REPLY_INFO =
        "Quotes are setup. To join another channel, run the following command in that channel and follow on-screen instructions: !quotejoin {ChannelId}";

    private const string REPLY_INFO_MD =
        "Quotes are setup. To join another channel, run the following command in that channel and follow on-screen instructions: `!quotejoin {ChannelId}`";

    private const string REPLY_INFO_NOT_SETUP =
        "Quotes are not setup. To join this channel to an existing channel with quotes, run !quoteinfo on a channel that is already setup.";

    private const string REPLY_JOIN =
        "Join process started. To confirm the link, run the following command in one of your already joined channels: \"!quoteconfirm {ChannelId} {JoinCode}\". " +
        "Please note that this code will expire in 5 minutes.";

    private const string REPLY_JOIN_MD =
        "Join process started. To confirm the link, run the following command in one of your already joined channels: `!quoteconfirm {ChannelId} {JoinCode}`. " +
        "Please note that this code will expire in 5 minutes.";

    private const string REPLY_JOIN_FAILED =
        "Failed to start join process. The provided channel is either not known or already joined.";

    private const string REPLY_CONFIRM_FAILED =
        "Failed to confirm channel. Make sure your code was copied correctly and that it has not been more than " +
        "5 minutes since you started the join process with !quotejoin.";

    private const string REPLY_CONFIRM = "Join confirmed! {ChannelId} now shares quotes with {OtherChannelId}.";

    private readonly ILogger<QuoteLinkCommands> _logger;
    private readonly IQuoteChannelMappingService _quoteChannelMappingService;

    public QuoteLinkCommands(
        ILogger<QuoteLinkCommands> logger,
        IQuoteChannelMappingService quoteChannelMappingService)
    {
        _logger = logger;
        _quoteChannelMappingService = quoteChannelMappingService;
    }

    [Command("quoteinfo", "qi")]
    public string? GetMappingInfo(
        [SourceChannel] string sourceChannel,
        [SupportedFeatures] ChatInterfaceFeatures supportedFeatures)
    {
        var mapping = _quoteChannelMappingService.GetQuoteChannelMapping(sourceChannel);
        if (mapping == null)
        {
            return REPLY_INFO_NOT_SETUP;
        }

        var msg = supportedFeatures.Supports(ChatInterfaceFeatures.MarkdownText) ? REPLY_INFO_MD : REPLY_INFO;
        return msg.Format(new
        {
            ChannelId = sourceChannel
        });
    }

    [Command("quotejoin", "qj")]
    public string? JoinChannel(
        [SourceChannel] string sourceChannel,
        [SupportedFeatures] ChatInterfaceFeatures supportedFeatures,
        [ArgumentIndex(0)] string joinToChannel)
    {
        var mapping = _quoteChannelMappingService.GetQuoteChannelMapping(joinToChannel);
        if (mapping == null)
        {
            return REPLY_JOIN_FAILED;
        }

        var joinCode = _quoteChannelMappingService.StartJoinProcess(mapping, sourceChannel);
        if (joinCode == null)
        {
            return REPLY_JOIN_FAILED;
        }

        return (supportedFeatures.Supports(ChatInterfaceFeatures.MarkdownText) ? REPLY_JOIN_MD : REPLY_JOIN).Format(new
        {
            ChannelId = sourceChannel,
            JoinCode = joinCode
        });
    }

    [Command("quoteconfirm", "qc")]
    public string? ConfirmJoin(
        [SourceChannel] string sourceChannel,
        [ArgumentIndex(0)] string channelId,
        [ArgumentIndex(1)] string joinCode)
    {
        var mapping = _quoteChannelMappingService.GetQuoteChannelMapping(sourceChannel);
        if (mapping == null)
        {
            return null;
        }

        if (!_quoteChannelMappingService.ConfirmJoinProcess(mapping, channelId, joinCode))
        {
            return REPLY_CONFIRM_FAILED;
        }

        return REPLY_CONFIRM.Format(new
        {
            ChannelId = channelId,
            OtherChannelId = sourceChannel
        });
    }
}
