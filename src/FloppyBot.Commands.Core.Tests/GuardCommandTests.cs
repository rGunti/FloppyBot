using System.Collections.Immutable;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes.Guards;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Executor;
using FloppyBot.Commands.Core.Guard;
using FloppyBot.Commands.Core.Scan;
using FloppyBot.Commands.Core.Spawner;
using FloppyBot.Commands.Core.Support;
using FloppyBot.Commands.Core.Support.PreExecution;
using FloppyBot.Commands.Core.Tests.Impl;
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
            .AddSingleton<ICommandSpawner, CommandSpawner>()
            .AddSingleton<ICommandExecutor, CommandExecutor>()
            .AddGuardRegistry()
            .AddGuard<PrivilegeGuard, PrivilegeGuardAttribute>()
            .AddPreExecutionTask<GuardTask>()
            .BuildServiceProvider();
    }

    private ICommandExecutor CommandExecutor => _serviceProvider.GetRequiredService<ICommandExecutor>();

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
}
