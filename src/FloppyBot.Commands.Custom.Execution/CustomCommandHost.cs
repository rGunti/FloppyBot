using System.Collections.Immutable;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Scan;
using FloppyBot.Commands.Custom.Communication;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.Commands.Custom.Storage.Entities.Internal;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Custom.Execution;

[VariableCommandHost]
public class CustomCommandHost
{
    private readonly ICustomCommandExecutor _commandExecutor;
    private readonly ICustomCommandService _commandService;
    private readonly ILogger<CustomCommandHost> _logger;

    public CustomCommandHost(
        ILogger<CustomCommandHost> logger,
        ICustomCommandExecutor commandExecutor,
        ICustomCommandService commandService
    )
    {
        _logger = logger;
        _commandExecutor = commandExecutor;
        _commandService = commandService;
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void DiSetup(IServiceCollection services)
    {
        WebDiSetup(services);
        services
            .AddCommandListSupplier<CustomCommandListSupplier>()
            .AddScoped<ICustomCommandExecutor, CustomCommandExecutor>()
            .AddScoped<ISoundCommandInvocationSender, SoundCommandInvocationSender>();
    }

    public static void WebDiSetup(IServiceCollection services)
    {
        services
            .AddAutoMapper(typeof(CustomCommandStorageProfile))
            .AddScoped<ICustomCommandService, CustomCommandService>()
            .AddScoped<ICounterStorageService, CounterStorageService>()
            .AddSingleton<ISoundCommandInvocationReceiver, SoundCommandInvocationReceiver>();
    }

    public bool CanHandleCommand(CommandInstruction instruction)
    {
        return GetCommand(instruction).HasValue;
    }

    [VariableCommandHandler(nameof(CanHandleCommand), identifier: "Custom Commands")]
    // ReSharper disable once UnusedMember.Global
    public CommandResult RunCustomCommand(CommandInstruction instruction)
    {
        CustomCommandDescription customCommand = GetCommand(instruction)
            .OrThrow(() => CreateCommandNotFoundException(instruction));
        ImmutableList<CommandResult?> replies = _commandExecutor
            .Execute(instruction, customCommand)
            .ToImmutableList();

        if (replies.IsEmpty)
        {
            return new CommandResult(CommandOutcome.NoResponse);
        }

        // TODO: Supply multiple results
        var responseMessage = replies
            .Where(r => r is not null && r.HasResponse)
            .Select(r => r!.ResponseContent)
            .Join("\n\n");
        var sendResponseAsReply = replies
            .Where(r => r is not null && r.HasResponse)
            .Select(r => r!.SendAsReply)
            .FirstOrDefault(true);
        return new CommandResult(CommandOutcome.Success, responseMessage, sendResponseAsReply);
    }

    private NullableObject<CustomCommandDescription> GetCommand(CommandInstruction instruction)
    {
        return _commandService
            .GetCommand(
                instruction.Context!.SourceMessage!.Identifier.GetChannel(),
                instruction.CommandName
            )
            .Wrap();
    }

    private Exception CreateCommandNotFoundException(CommandInstruction instruction)
    {
        return new KeyNotFoundException(
            $"Channel={instruction.Context!.SourceMessage!.Identifier.GetChannel()}, Command={instruction.CommandName}"
        );
    }
}
