using System.Collections.Immutable;
using System.Reflection;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Support;
using FloppyBot.Commands.Core.Support.PostExecution;
using FloppyBot.Commands.Core.Support.PreExecution;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Core.Spawner;

public class CommandSpawner : ICommandSpawner
{
    private static readonly IImmutableDictionary<Type, Func<string, object>> StringTypeConversions =
        new Dictionary<Type, Func<string, object>>
        {
            { typeof(int), s => Convert.ToInt32(s) },
            { typeof(long), s => Convert.ToInt64(s) },
            { typeof(double), s => Convert.ToDouble(s) },
            { typeof(float), s => Convert.ToSingle(s) },
            { typeof(decimal), s => Convert.ToDecimal(s) },
            { typeof(byte), s => Convert.ToByte(s) }
        }.ToImmutableDictionary();

    private readonly ILogger<CommandSpawner> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CommandSpawner(
        ILogger<CommandSpawner> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public ChatMessage? SpawnAndExecuteCommand(CommandInfo commandInfo, CommandInstruction instruction)
    {
        using var logScope = _logger.BeginScope(instruction.Context!.SourceMessage.Identifier);
        using var scope = _serviceProvider.CreateScope();
        _logger.LogTrace("Created new service provider scope");

        object? host = null;
        if (!commandInfo.IsStatic)
        {
            _logger.LogDebug("Creating instance of host class {HostType}", commandInfo.ImplementingType);
            try
            {
                host = scope.ServiceProvider.GetRequiredService(commandInfo.ImplementingType);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create host class {HostType} due to an exception",
                    commandInfo.ImplementingType);
                throw;
            }
        }
        else
        {
            _logger.LogDebug("Skipped creating host class instance because command is declared as static");
        }

        _logger.LogDebug("Running pre-execution tasks");
        IPreExecutionTask? failedPreExecutionTask = TryExtensions.TryOr(
            () => scope.RunPreExecutionTasks(commandInfo, instruction),
            e => { _logger.LogWarning(e, "Post-execution tasks failed due to an exception"); });
        if (failedPreExecutionTask != null)
        {
            _logger.LogInformation(
                "Pre-execution task {TaskName} has failed, command is not executed",
                failedPreExecutionTask.GetType());
            return null;
        }

        _logger.LogDebug("Building arguments");
        object?[] commandArguments;
        try
        {
            commandArguments = ConstructArguments(commandInfo.HandlerMethod, instruction);
        }
        catch (ArgumentException ex)
        {
            _logger.LogInformation(
                ex,
                "Could not parse arguments due to an exception (possibly not enough arguments supplied)");
            return null;
        }
        catch (InvalidCastException ex)
        {
            _logger.LogInformation(
                ex,
                "Could not cast data to arguments (possibly wrong data type for argument)");
            return null;
        }

        _logger.LogInformation(
            "Executing command {@CommandHandler} with {CommandArgsCount} arguments",
            commandInfo.HandlerMethod,
            commandArguments.Length);

        object? returnValue = commandInfo.HandlerMethod.Invoke(host, commandArguments);
        CommandResult result = ProcessReturnValue(returnValue);

        _logger.LogDebug("Running post-execution tasks");
        IPostExecutionTask? failedPostExecutionTask = TryExtensions.TryOr(
            () => scope.RunPostExecutionTasks(commandInfo, instruction, result),
            e => { _logger.LogWarning(e, "Post-execution tasks failed due to an exception"); });
        if (failedPostExecutionTask != null)
        {
            _logger.LogInformation(
                "Post-execution task {TaskName} has failed, response is dropped",
                failedPostExecutionTask.GetType());
            return null;
        }

        _logger.LogDebug("Command executed successfully, returning value");
        return result.HasResponse ? instruction.CreateReply(result.ResponseContent!) : null;
    }

    public bool CanExecuteVariableCommand(VariableCommandInfo commandInfo, CommandInstruction instruction)
    {
        using var logScope = _logger.BeginScope(instruction.Context!.SourceMessage.Identifier);
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

        _logger.LogInformation(
            "Executing command assertion handler {@CommandHandler}",
            commandInfo.TestHandlerMethod);
        return (bool)commandInfo.TestHandlerMethod.Invoke(host, new object?[]
        {
            instruction
        })!;
    }

    private CommandResult ProcessReturnValue(object? returnValue)
    {
        if (returnValue == null)
        {
            _logger.LogDebug("Return value was null, returning null as well");
            return new CommandResult(CommandOutcome.NoResponse);
        }

        var returnValueToProcess = returnValue;
        if (returnValueToProcess is Task task)
        {
            _logger.LogDebug("Return value is asynchronous, waiting for reply");
            task.ConfigureAwait(false);
            task.Wait();
            // ReSharper disable once TailRecursiveCall
            returnValueToProcess = (object)((dynamic)task).Result;
        }

        switch (returnValueToProcess)
        {
            case string returnMessage:
                _logger.LogDebug("Return value was string, returning as successful outcome");
                return new CommandResult(CommandOutcome.Success, returnMessage);
            case CommandResult result:
                _logger.LogDebug($"Return value was {nameof(CommandResult)}, returning as is");
                return result;
            case ChatMessage chatMessage:
                _logger.LogWarning(
                    "Return value was chat message (deprecated), returning its content as successful outcome");
                return new CommandResult(CommandOutcome.Success, chatMessage.Content);
            default:
                _logger.LogError("Return value was of type {ReturnValueType}, which is not supported",
                    returnValueToProcess.GetType());
                throw new InvalidDataException(
                    $"Return value was of type {returnValueToProcess.GetType()}, which is not supported");
        }
    }

    private static object?[] ConstructArguments(
        MethodInfo methodInfo,
        CommandInstruction instruction)
    {
        return methodInfo.GetParameters()
            .Select(p =>
            {
                var argValue = ConstructArgument(p, instruction);
                if (!p.ParameterType.IsAssignableFrom(argValue?.GetType() ?? typeof(object)))
                {
                    return ConvertArgumentTo(argValue, p.ParameterType);
                }

                return argValue;
            })
            .ToArray();
    }

    private static object? ConstructArgument(ParameterInfo parameterInfo, CommandInstruction instruction)
    {
        if (parameterInfo.ParameterType == typeof(CommandInstruction))
        {
            return instruction;
        }

        var argumentAttribute = parameterInfo.GetCustomAttribute<BaseArgumentAttribute>();
        if (argumentAttribute == null)
        {
            throw new ArgumentException(
                $"Don't know how to extract argument {parameterInfo}. Have you added an attribute?");
        }

        return argumentAttribute.ExtractArgument(parameterInfo, instruction);
    }

    private static object? ConvertArgumentTo(object? sourceValue, Type targetType)
    {
        if (sourceValue == null)
        {
            return null;
        }

        if (sourceValue is string sourceValueString)
        {
            if (StringTypeConversions.ContainsKey(targetType))
            {
                return StringTypeConversions[targetType].Invoke(sourceValueString);
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, sourceValueString);
            }

            throw new InvalidCastException($"Cannot (yet) convert from string to {targetType}");
        }

        throw new InvalidCastException($"Cannot yet convert from {sourceValue.GetType()} to {targetType}");
    }
}

