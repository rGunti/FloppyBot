using System.Collections.Immutable;
using FloppyBot.Base.Extensions;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Dependencies;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Scan;
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
        ICustomCommandService commandService)
    {
        _logger = logger;
        _commandExecutor = commandExecutor;
        _commandService = commandService;
    }

    private NullableObject<CustomCommandDescription> GetCommand(CommandInstruction instruction)
    {
        return _commandService.GetCommand(
                instruction.Context!.SourceMessage!.Identifier.GetChannel(),
                instruction.CommandName)
            .Wrap();
    }

    private Exception CreateCommandNotFoundException(CommandInstruction instruction)
    {
        return new KeyNotFoundException(
            $"Channel={instruction.Context!.SourceMessage!.Identifier.GetChannel()}, Command={instruction.CommandName}");
    }

    public bool CanHandleCommand(CommandInstruction instruction)
    {
        return GetCommand(instruction).HasValue;
    }

    [VariableCommandHandler(
        nameof(CanHandleCommand),
        identifier: "Custom Commands")]
    // ReSharper disable once UnusedMember.Global
    public CommandResult? RunCustomCommand(CommandInstruction instruction)
    {
        CustomCommandDescription customCommand = GetCommand(instruction)
            .OrThrow(() => CreateCommandNotFoundException(instruction));
        ImmutableList<string?> replies = _commandExecutor.Execute(instruction, customCommand)
            .ToImmutableList();

        if (!replies.Any())
        {
            return new CommandResult(CommandOutcome.NoResponse);
        }

        // TODO: Supply multiple results
        return new CommandResult(
            CommandOutcome.Success,
            replies.Join("\n\n"));
    }

    [DependencyRegistration]
    // ReSharper disable once UnusedMember.Global
    public static void DiSetup(IServiceCollection services)
    {
        services
            .AddAutoMapper(typeof(CustomCommandStorageProfile))
            .AddCommandListSupplier<CustomCommandListSupplier>()
            .AddScoped<ICustomCommandExecutor, CustomCommandExecutor>()
            .AddScoped<ICustomCommandService, CustomCommandService>();
    }

    public static void WebDiSetup(IServiceCollection services)
    {
        services
            .AddAutoMapper(typeof(CustomCommandStorageProfile))
            .AddScoped<ICustomCommandService, CustomCommandService>();
    }
}
