using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Executor.Agent.Cmds;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Executor.Agent.Utils;

internal static class CommandUtils
{
    public static IServiceCollection AddCommands(this IServiceCollection services)
    {
        return services
            .AddCommand<PingCommand>()
            .AddCommand<MathCommand>();
    }

    private static IServiceCollection AddCommand<T>(this IServiceCollection services) where T : class, IBotCommand
    {
        return services
            .AddScoped<IBotCommand, T>();
    }

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
}
