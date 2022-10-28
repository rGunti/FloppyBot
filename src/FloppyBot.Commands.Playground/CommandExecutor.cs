using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.Logging;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Playground.Attributes;
using FloppyBot.Commands.Playground.Guards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FloppyBot.Commands.Playground;

public class CommandExecutor
{
    private readonly IImmutableDictionary<string, MethodInfo> _handlerIndex;
    private readonly ILogger<CommandExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CommandExecutor(
        ILogger<CommandExecutor> logger,
        IServiceProvider serviceProvider,
        IImmutableDictionary<string, MethodInfo> handlers)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _handlerIndex = handlers;
    }

    public static CommandExecutor Create()
    {
        var serilogger = new LoggerConfiguration()
            .ConfigureSerilogForTesting()
            .CreateLogger();

        var di = new ServiceCollection()
            .AddLogging(l => l.AddSerilog(serilogger));

        var types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.GetCustomAttributes<CommandHostAttribute>().Any())
            .Distinct();
        var commandHandlers = new Dictionary<string, MethodInfo>();
        foreach (var type in types)
        {
            var handlersOnType = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttributes<CommandAttribute>().Any());

            foreach (var handler in handlersOnType)
            {
                var commandInfo = handler.GetCustomAttributes<CommandAttribute>()
                    .First();
                foreach (var commandName in commandInfo.CommandNames)
                {
                    commandHandlers.Add(
                        commandName,
                        handler);
                }
            }

            di.AddScoped(type);
        }

        di.AddScoped<ModeratorGuard>();
        di.AddScoped<DisabledGuard>();

        di.AddSingleton<IImmutableDictionary<string, MethodInfo>>(commandHandlers.ToImmutableDictionary());
        di.AddSingleton<CommandExecutor>();

        var sp = di.BuildServiceProvider();
        return sp.GetRequiredService<CommandExecutor>();
    }

    public ChatMessage? TryExecute(CommandInstruction instruction)
    {
        _logger.LogInformation("Trying to execute command with name {CommandName}", instruction.CommandName);
        var method = _handlerIndex[instruction.CommandName];

        _logger.LogDebug("Found handler {@CommandMethod}", method);
        _logger.LogDebug("Creating new scope to construct command handler host {CommandHostType} if required",
            method.ReflectedType);

        using var scope = _serviceProvider.CreateScope();
        var host = method.IsStatic ? null : scope.ServiceProvider.GetRequiredService(method.ReflectedType!);

        _logger.LogDebug("Executing guards ...");
        var firstFailedGuard = method.GetCustomAttributes<GuardAttribute>()
            .Select(g => g.Type)
            .Select(guardType =>
            {
                _logger.LogDebug("Creating guard {GuardType}", guardType);
                return scope.ServiceProvider.GetRequiredService(guardType);
            })
            .Cast<ICommandGuard>()
            .FirstOrDefault(g =>
            {
                _logger.LogDebug("Executing guard {GuardType}", g.GetType());
                return !g.CanExecute(instruction);
            });

        if (firstFailedGuard != null)
        {
            _logger.LogInformation("Guard {GuardType} failed, cannot execute command", firstFailedGuard.GetType());
            return null;
        }

        _logger.LogInformation("Executing command {CommandName} with {@CommandMethod}",
            instruction.CommandName,
            method);
        var parameters = ConstructParameters(instruction, method);
        var returnValue = method.Invoke(host, parameters);

        return (ChatMessage?)returnValue;
    }

    private static object?[] ConstructParameters(
        CommandInstruction instruction,
        MethodBase methodInfo)
    {
        var list = new List<object?>
        {
            instruction
        };

        var parameters = methodInfo.GetParameters()
            .Skip(1)
            .ToArray();

        if (parameters.Any())
        {
            list.AddRange(parameters
                .Select(parameter => parameter.GetCustomAttribute<CommandParameterAttribute>())
                .Select(attribute => attribute?.Parse(instruction.Parameters)));
        }

        return list.ToArray();
    }
}
