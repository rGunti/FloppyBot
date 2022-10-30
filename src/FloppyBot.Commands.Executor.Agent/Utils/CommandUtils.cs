using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;

namespace FloppyBot.Commands.Executor.Agent.Utils;

internal static class CommandUtils
{
    public static string JoinToString(this IEnumerable<string> stringEnumerable)
    {
        return string.Join(" ", stringEnumerable);
    }

    public static ChatMessage Reply(
        this CommandInstruction commandInstruction,
        string reply)
    {
        return commandInstruction.Context!.SourceMessage with
        {
            Content = reply
        };
    }

    public static ChatMessage? ReplyIfNotEmpty(
        this CommandInstruction commandInstruction,
        string? reply)
    {
        return !string.IsNullOrWhiteSpace(reply) ? commandInstruction.Reply(reply) : null;
    }

    public static bool SourceSupports(this CommandInstruction commandInstruction, ChatInterfaceFeatures feature)
    {
        return commandInstruction.Context!.SourceMessage.SupportedFeatures.Supports(feature);
    }

    public static string DetermineMessageTemplate(
        this CommandInstruction commandInstruction,
        ChatInterfaceFeatures features,
        string messageTemplate,
        string defaultTemplate)
    {
        return commandInstruction.SourceSupports(features) ? messageTemplate : defaultTemplate;
    }
}
