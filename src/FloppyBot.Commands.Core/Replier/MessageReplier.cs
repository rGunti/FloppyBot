using System.Collections.Concurrent;
using FloppyBot.Base.Configuration;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Replier;

public class MessageReplier : IMessageReplier
{
    private readonly ILogger<MessageReplier> _logger;
    private readonly string _senderConnectionString;
    private readonly INotificationSenderFactory _senderFactory;

    private readonly ConcurrentDictionary<string, INotificationSender> _senders;

    public MessageReplier(
        INotificationSenderFactory senderFactory,
        IConfiguration configuration,
        ILogger<MessageReplier> logger
    )
    {
        _senderFactory = senderFactory;
        _logger = logger;
        _senderConnectionString = configuration.GetParsedConnectionString("ResponseOutput");
        _senders = new ConcurrentDictionary<string, INotificationSender>();
    }

    public void SendMessage(ChatMessage chatMessage)
    {
        _logger.LogInformation(
            "Sending message to {MessageInterface}",
            chatMessage.Identifier.Interface
        );
        _logger.LogDebug("Sending message {MessageIdentifier}", chatMessage.Identifier);
        INotificationSender sender = GetSenderForInterface(chatMessage.Identifier.Interface);
        sender.Send(chatMessage);
    }

    private INotificationSender GetSenderForInterface(string messageInterface)
    {
        _logger.LogTrace("Getting sender for {MessageInterface}", messageInterface);
        return _senders.GetOrAdd(
            messageInterface,
            msgIf =>
            {
                _logger.LogDebug(
                    "Creating new sender for message interface {MessageInterface}",
                    messageInterface
                );
                return _senderFactory.GetNewSender(
                    _senderConnectionString.Format(new { Interface = messageInterface })
                );
            }
        );
    }
}
