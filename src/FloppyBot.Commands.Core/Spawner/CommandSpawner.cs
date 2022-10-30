using System.Reflection;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Spawner;

public class CommandSpawner : ICommandSpawner
{
    private readonly ILogger<CommandSpawner> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CommandSpawner(ILogger<CommandSpawner> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public ChatMessage? SpawnAndExecuteCommand(CommandInfo commandInfo, CommandInstruction instruction)
    {
        using var scope = _serviceProvider.CreateScope();
        _logger.LogTrace("Created new service provider scope");

        object? host = null;
        if (!commandInfo.IsStatic)
        {
            _logger.LogDebug("Creating instance of host class {HostType}", commandInfo.ImplementingType);
            host = scope.ServiceProvider.GetRequiredService(commandInfo.ImplementingType);
        }
        else
        {
            _logger.LogDebug("Skipped creating host class instance because command is declared as static");
        }

        // TODO: Guard

        _logger.LogDebug("Building arguments");
        var commandArguments = ConstructArguments(commandInfo.HandlerMethod, instruction);

        _logger.LogInformation(
            "Executing command {@CommandHandler} with {CommandArgsCount} arguments",
            commandInfo.HandlerMethod,
            commandArguments.Length);

        var returnValue = commandInfo.HandlerMethod.Invoke(host, commandArguments);

        _logger.LogDebug("Command executed successfully, returning value");
        switch (returnValue)
        {
            case null:
                _logger.LogDebug("Return value was null, returning null as well");
                return null;
            case string returnMessage:
                _logger.LogDebug("Return value was string, creating new reply and returning it");
                return instruction.CreateReply(returnMessage);
            case ChatMessage chatMessage:
                _logger.LogDebug("Return value was ChatMessage, returning it");
                return chatMessage;
            default:
                _logger.LogError("Return value was of type {ReturnValueType}, which is not supported",
                    returnValue.GetType());
                throw new InvalidDataException(
                    $"Return value was of type {returnValue.GetType()}, which is not supported");
        }
    }

    private static object?[] ConstructArguments(
        MethodInfo methodInfo,
        CommandInstruction instruction)
    {
        return methodInfo.GetParameters()
            .Select(p => ConstructArgument(p, instruction))
            .ToArray();
    }

    private static object? ConstructArgument(ParameterInfo parameterInfo, CommandInstruction instruction)
    {
        if (parameterInfo.ParameterType == typeof(CommandInstruction))
        {
            return instruction;
        }

        return null;
    }
}
