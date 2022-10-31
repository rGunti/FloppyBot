using FloppyBot.Base.EquatableCollections;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Core.Exceptions;
using FloppyBot.Commands.Core.Scan;
using FloppyBot.Commands.Core.Tests.Impl;

namespace FloppyBot.Commands.Core.Tests;

[TestClass]
public class CommandScannerTests
{
    private readonly ICommandScanner _scanner;

    public CommandScannerTests()
    {
        _scanner = new CommandScanner();
    }

    [TestMethod]
    public void DiscoversHandlersCorrectly()
    {
        var commands = _scanner.ScanTypeForCommandHandlers(typeof(SampleCommands))
            .ToArray();

        foreach (var commandInfo in commands)
        {
            Console.WriteLine(commandInfo);
        }

        CollectionAssert.AreEquivalent(
            new[]
            {
                new CommandInfo(
                    new[] { "ping" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.Ping))!),
                new CommandInfo(
                    new[] { "sping" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.StaticPing))!),
                new CommandInfo(
                    new[] { "simple" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.Simple))!),
                new CommandInfo(
                    new[] { "noargs" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.NoArgsCommand))!),
                new CommandInfo(
                    new[] { "args" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.ArgsCommand))!),
                new CommandInfo(
                    new[] { "add" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.Add))!),
                new CommandInfo(
                    new[] { "list" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.List))!),
                new CommandInfo(
                    new[] { "allargs" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.AllArgs))!),
                new CommandInfo(
                    new[] { "allargs1" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.AllArgsAsString))!),
                new CommandInfo(
                    new[] { "enum" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.Enum))!),
                new CommandInfo(
                    new[] { "author" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.AuthorName))!),
                new CommandInfo(
                    new[] { "feature" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.SupportFeatures))!),
            },
            commands);
    }

    [TestMethod]
    public void ThrowsExceptionWhenNotMarkedAsCommandHost()
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            _scanner.ScanTypeForCommandHandlers(typeof(NotACommandHost));
        });
    }

    [TestMethod]
    public void IgnoresClassesWhenNotMarkedAsCommandHost()
    {
        var commands = _scanner.ScanTypesForCommandHandlers(new[]
        {
            typeof(SampleCommands),
            typeof(NotACommandHost)
        });

        Assert.IsFalse(commands.Any(c => c.ImplementingType == typeof(NotACommandHost)));
    }

    [TestMethod]
    public void ThrowsAtInvalidCommandNames()
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            var _ = _scanner.ScanTypeForCommandHandlers(typeof(InvalidCommandName)).ToArray();
        });
    }

    [TestMethod]
    public void ThrowsAtInvalidCommandSignatures()
    {
        Assert.ThrowsException<InvalidCommandSignatureException>(() =>
        {
            var _ = _scanner.ScanTypeForCommandHandlers(typeof(InvalidCommandSignature)).ToArray();
        });
    }
}
