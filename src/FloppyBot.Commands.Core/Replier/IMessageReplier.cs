using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Replier;

public interface IMessageReplier
{
    void SendMessage(ChatMessage chatMessage);
}
