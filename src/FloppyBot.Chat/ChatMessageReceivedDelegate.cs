using FloppyBot.Chat.Entities;

namespace FloppyBot.Chat;

public delegate void ChatMessageReceivedDelegate(ChatMessage chatMessage, IChatInterface sourceInterface);
