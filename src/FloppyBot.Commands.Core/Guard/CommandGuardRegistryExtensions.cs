using FloppyBot.Commands.Core.Attributes.Guards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Guard;

public static class CommandGuardRegistryExtensions
{
    public static IServiceCollection AddGuardRegistry(this IServiceCollection services)
    {
        return services.AddSingleton<ICommandGuardRegistry, CommandGuardRegistry>(s =>
        {
            var inst = new CommandGuardRegistry(
                s.GetRequiredService<ILogger<CommandGuardRegistry>>()
            );
            foreach (
                var (attributeType, implementationType) in s.GetRequiredService<
                    IEnumerable<CommandGuardTypePair>
                >()
            )
            {
                inst.RegisterGuard(attributeType, implementationType);
            }

            return inst;
        });
    }

    public static IServiceCollection AddGuard<TGuard, TGuardAttribute>(
        this IServiceCollection services
    )
        where TGuard : class, ICommandGuard<TGuardAttribute>
        where TGuardAttribute : GuardAttribute
    {
        return services
            .AddSingleton<CommandGuardTypePair>(
                new CommandGuardTypePair(typeof(TGuardAttribute), typeof(TGuard))
            )
            .AddScoped<TGuard>();
    }
}
