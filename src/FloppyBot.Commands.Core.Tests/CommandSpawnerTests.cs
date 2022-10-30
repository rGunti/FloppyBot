using System.Text.Json;
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

    [TestMethod]
    public void ArgumentAttributesWorkAsExpected()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "args",
            new[]
            {
                "First",
                "Second",
                "Third",
                "Forth",
                "Fifth"
            });
        var command = GetCommandInfo(
            "args",
            typeof(SampleCommands),
            nameof(SampleCommands.ArgsCommand));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsInstanceOfType(returnValue, typeof(ChatMessage));
        Assert.AreEqual(
            JsonSerializer.Serialize(new
            {
                arg0 = "First",
                arg1 = "Second Third Forth"
            }),
            returnValue!.Content);
    }

    [TestMethod]
    public void MissingArgumentsWillCauseMessageToBeSkip()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "args",
            Array.Empty<string>());
        var command = GetCommandInfo(
            "args",
            typeof(SampleCommands),
            nameof(SampleCommands.ArgsCommand));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsNull(returnValue);
    }

    [TestMethod]
    public void CanConvertArguments()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "add",
            new[] { "1", "2" });
        var command = GetCommandInfo(
            "add",
            typeof(SampleCommands),
            nameof(SampleCommands.Add));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsInstanceOfType(returnValue, typeof(ChatMessage));
        Assert.AreEqual(
            "3",
            returnValue!.Content);
    }

    [TestMethod]
    public void CanHandleListArguments()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "list",
            new[] { "1", "2", "3", "4", "5" });
        var command = GetCommandInfo(
            "list",
            typeof(SampleCommands),
            nameof(SampleCommands.List));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsInstanceOfType(returnValue, typeof(ChatMessage));
        Assert.AreEqual(
            "1,2,3,4,5",
            returnValue!.Content);
    }

    [TestMethod]
    public void CanHandleAllArguments()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "allargs",
            new[] { "1", "2", "3", "4", "5" });
        var command = GetCommandInfo(
            "allargs",
            typeof(SampleCommands),
            nameof(SampleCommands.AllArgs));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsInstanceOfType(returnValue, typeof(ChatMessage));
        Assert.AreEqual(
            "1/2/3/4/5",
            returnValue!.Content);
    }

    [DataTestMethod]
    [DataRow(SampleCommands.SampleEnum.A)]
    [DataRow(SampleCommands.SampleEnum.B)]
    [DataRow(SampleCommands.SampleEnum.C)]
    public void CanHandleEnumArgument(SampleCommands.SampleEnum sampleEnumValue)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "enum",
            new[] { $"{sampleEnumValue}" });
        var command = GetCommandInfo(
            "enum",
            typeof(SampleCommands),
            nameof(SampleCommands.Enum));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsInstanceOfType(returnValue, typeof(ChatMessage));
        Assert.AreEqual(
            $"Enum value was: {sampleEnumValue}",
            returnValue!.Content);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("nope")]
    public void CanHandleInvalidEnumArgument(string input)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "enum",
            new[] { input });
        var command = GetCommandInfo(
            "enum",
            typeof(SampleCommands),
            nameof(SampleCommands.Enum));
        var spawner = GetCommandSpawner<SampleCommands>();

        var returnValue = spawner.SpawnAndExecuteCommand(command, instruction);

        Assert.IsNull(returnValue);
    }
}
