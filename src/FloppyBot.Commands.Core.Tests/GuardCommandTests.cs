using System.Collections.Immutable;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Guard;
using FloppyBot.Commands.Core.Scan;
using FloppyBot.Commands.Core.Spawner;
using FloppyBot.Commands.Core.Support;
using FloppyBot.Commands.Core.Support.PreExecution;
using FloppyBot.Commands.Core.Tests.Impl;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FloppyBot.Commands.Core.Tests;

[TestClass]
public class GuardCommandTests
{
    private readonly IServiceProvider _serviceProvider;

    public GuardCommandTests()
    {
        var scanner = new CommandScanner();
        _serviceProvider = new ServiceCollection()
            .AddLogging(lb => lb.AddSerilog(LoggingUtils.SerilogLogger.Value))
            .AddSingleton(scanner.IndexCommands(
                scanner.ScanTypeForCommandHandlers<GuardedCommands>()))
            .AddSingleton<IImmutableList<VariableCommandInfo>>(Array.Empty<VariableCommandInfo>().ToImmutableList())
            .AddScoped<GuardedCommands>()
            .AddInMemoryStorage()
            .AddScoped<ICommandConfigurationService, CommandConfigurationService>()
            .AddSingleton<ICommandSpawner, CommandSpawner>()
            .AddSingleton<ICommandExecutor, CommandExecutor>()
            .AddGuardRegistry()
            .AddGuard<PrivilegeGuard, PrivilegeGuardAttribute>()
            .AddPreExecutionTask<GuardTask>()
            .BuildServiceProvider();
    }

    private ICommandExecutor CommandExecutor => _serviceProvider.GetRequiredService<ICommandExecutor>();

    private ICommandConfigurationService CommandConfigurationService
        => _serviceProvider.GetRequiredService<ICommandConfigurationService>();

    [DataTestMethod]
    [DataRow(PrivilegeLevel.Unknown, false)]
    [DataRow(PrivilegeLevel.Viewer, false)]
    [DataRow(PrivilegeLevel.Moderator, false)]
    [DataRow(PrivilegeLevel.Administrator, true)]
    [DataRow(PrivilegeLevel.Superuser, true)]
    public void PrivilegeGuardWorks(
        PrivilegeLevel privilegeLevelToTest,
        bool expectExecution)
    {
        var reply = CommandExecutor.ExecuteCommand(MockCommandFactory.NewInstruction(
            "adminonly",
            Array.Empty<string>(),
            privilegeLevelToTest));

        if (expectExecution)
        {
            Assert.IsNotNull(reply, "Command was not executed");
            Assert.AreEqual("Only for Admins!", reply.Content);
        }
        else
        {
            Assert.IsNull(reply, "Command was executed");
        }
    }

    [DataTestMethod]
    // No config
    [DataRow(PrivilegeLevel.Unknown, null, false)]
    [DataRow(PrivilegeLevel.Viewer, null, false)]
    [DataRow(PrivilegeLevel.Moderator, null, false)]
    [DataRow(PrivilegeLevel.Administrator, null, true)]
    [DataRow(PrivilegeLevel.Superuser, null, true)]
    // Superuser (effectively disabled)
    [DataRow(PrivilegeLevel.Unknown, PrivilegeLevel.Superuser, false)]
    [DataRow(PrivilegeLevel.Viewer, PrivilegeLevel.Superuser, false)]
    [DataRow(PrivilegeLevel.Moderator, PrivilegeLevel.Superuser, false)]
    [DataRow(PrivilegeLevel.Administrator, PrivilegeLevel.Superuser, false)]
    [DataRow(PrivilegeLevel.Superuser, PrivilegeLevel.Superuser, true)]
    // Unknown (effectively everyone)
    [DataRow(PrivilegeLevel.Unknown, PrivilegeLevel.Unknown, true)]
    [DataRow(PrivilegeLevel.Viewer, PrivilegeLevel.Unknown, true)]
    [DataRow(PrivilegeLevel.Moderator, PrivilegeLevel.Unknown, true)]
    [DataRow(PrivilegeLevel.Administrator, PrivilegeLevel.Unknown, true)]
    [DataRow(PrivilegeLevel.Superuser, PrivilegeLevel.Unknown, true)]
    // Moderator
    [DataRow(PrivilegeLevel.Unknown, PrivilegeLevel.Moderator, false)]
    [DataRow(PrivilegeLevel.Viewer, PrivilegeLevel.Moderator, false)]
    [DataRow(PrivilegeLevel.Moderator, PrivilegeLevel.Moderator, true)]
    [DataRow(PrivilegeLevel.Administrator, PrivilegeLevel.Moderator, true)]
    [DataRow(PrivilegeLevel.Superuser, PrivilegeLevel.Moderator, true)]
    public void PrivilegeGuardWorksWithConfiguredOverride(
        PrivilegeLevel userPrivilegeLevel,
        PrivilegeLevel? configuredPrivilegeLevel,
        bool expectExecution)
    {
        CommandInstruction instructions = MockCommandFactory.NewInstruction(
            "adminonly",
            Array.Empty<string>(),
            userPrivilegeLevel);
        CommandConfigurationService.SetCommandConfiguration(new CommandConfiguration
        {
            Id = "no-id",
            ChannelId = instructions.Context!.SourceMessage.Identifier.GetChannel(),
            CommandName = instructions.CommandName,
            RequiredPrivilegeLevel = configuredPrivilegeLevel,
            Disabled = false,
            CustomCooldown = false,
            CustomCooldownConfiguration = Array.Empty<CooldownConfiguration>(),
        });

        ChatMessage? reply = CommandExecutor.ExecuteCommand(instructions);

        if (expectExecution)
        {
            Assert.IsNotNull(reply, "Command was not executed");
            Assert.AreEqual("Only for Admins!", reply.Content);
        }
        else
        {
            Assert.IsNull(reply, "Command was executed");
        }
    }
}
