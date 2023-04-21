using System.Reflection;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Guard;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Support.PreExecution;

[TaskOrder(ORDER)]
public class GuardTask : IPreExecutionTask
{
    private const int ORDER = 10;
    private readonly ICommandGuardRegistry _guardRegistry;

    private readonly ILogger<GuardTask> _logger;
    private readonly IServiceProvider _provider;

    public GuardTask(
        ILogger<GuardTask> logger,
        IServiceProvider provider,
        ICommandGuardRegistry guardRegistry
    )
    {
        _logger = logger;
        _provider = provider;
        _guardRegistry = guardRegistry;
    }

    public bool ExecutePre(CommandInfo info, CommandInstruction instruction)
    {
        _logger.LogDebug("Fetching guard tasks to execute");
        var guards = info.ImplementingType
            .GetCustomAttributes<GuardAttribute>()
            .Concat(info.HandlerMethod.GetCustomAttributes<GuardAttribute>())
            .SelectMany(
                attribute =>
                    _guardRegistry
                        .FindGuardImplementation(attribute)
                        .Select(
                            guardType =>
                                new
                                {
                                    // ReSharper disable once AccessToDisposedClosure
                                    GuardImpl = (ICommandGuard)
                                        _provider.GetRequiredService(guardType),
                                    GuardType = guardType,
                                    Settings = attribute,
                                }
                        )
            )
            .ToArray();

        _logger.LogDebug("Found {GuardCount} guards to execute", guards.Length);
        var failedGuards = guards
            .Where(guard =>
            {
                _logger.LogTrace(
                    // ReSharper disable once ComplexObjectDestructuringProblem
                    "Running guard {GuardType} with settings {GuardSettings}",
                    guard.GuardType,
                    guard.Settings
                );
                return !guard.GuardImpl.CanExecute(instruction, info, guard.Settings);
            })
            .ToArray();

        if (failedGuards.Any())
        {
            _logger.LogInformation(
                "{FailedGuardCount} of {GuardCount} guard(s) have failed, command is not executed",
                failedGuards.Length,
                guards.Length
            );
            _logger.LogDebug(
                "The following guards have failed: {FailedGuards}",
                // ReSharper disable once CoVariantArrayConversion
                failedGuards.Select(g => g.Settings).ToArray()
            );
        }
        else
        {
            _logger.LogDebug("Passed {GuardCount} guards", guards.Length);
        }

        return !failedGuards.Any();
    }
}
