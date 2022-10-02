using FloppyBot.Chat.Entities;

namespace FloppyBot.Chat;

public delegate void ChatMessageReceivedDelegate(IChatInterface sourceInterface, ChatMessage chatMessage);
