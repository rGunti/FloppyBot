using FloppyBot.Base.EquatableCollections;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Spawner;
using FloppyBot.Commands.Core.Tests.Impl;
using FloppyBot.Commands.Parser.Entities.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace FloppyBot.Commands.Core.Tests;

[TestClass]
public class CommandSpawnerTests
{
    private static ICommandSpawner GetCommandSpawner<T>() where T : class
    {
        var sp = new ServiceCollection()
            .AddScoped<T>()
            .BuildServiceProvider();
        return new CommandSpawner(
            LoggingUtils.GetLogger<CommandSpawner>(),
            sp);
    }

    private static CommandInfo GetCommandInfo(
        string commandName,
        Type commandHostType,
        string methodName)
    {
        return new CommandInfo(
            new[] { commandName }.ToImmutableListWithValueSemantics(),
            commandHostType.GetMethod(methodName)!);
    }

    [TestMethod]
    public void RunsCommandAsExpected()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "ping",
            Array.Empty<string>());
        var command = GetCommandInfo(
            "ping",
            typeof(SampleCommands),
            nameof(SampleCommands.Ping));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsInstanceOfType(returnValue, typeof(ChatMessage));
        Assert.AreEqual("Pong!", returnValue!.Content);
    }

    [TestMethod]
    public void ReturnsReplyWithString()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "string",
            Array.Empty<string>());
        var command = GetCommandInfo(
            "string",
            typeof(SampleCommands),
            nameof(SampleCommands.Simple));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsInstanceOfType(returnValue, typeof(ChatMessage));
        Assert.AreEqual("Simple Response", returnValue!.Content);
    }

    [TestMethod]
    public void RunsCommandsWithNoArgs()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "ping",
            Array.Empty<string>());
        var command = GetCommandInfo(
            "ping",
            typeof(SampleCommands),
            nameof(SampleCommands.NoArgsCommand));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsInstanceOfType(returnValue, typeof(ChatMessage));
        Assert.AreEqual("No args at all", returnValue!.Content);
    }
}
