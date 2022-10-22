using FloppyBot.Commands.Executor.Agent.Cmds;
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
}
