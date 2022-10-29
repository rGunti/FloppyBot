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

        Assert.AreEqual(4, commands.Length);

        CollectionAssert.AreEquivalent(
            new[]
            {
                new CommandInfo(
                    new[] { "ping", "test" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.Ping))!),
                new CommandInfo(
                    new[] { "sping" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.StaticPing))!),
                new CommandInfo(
                    new[] { "simple" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.Simple))!),
                new CommandInfo(
                    new[] { "noargs" }.ToImmutableListWithValueSemantics(),
                    typeof(SampleCommands).GetMethod(nameof(SampleCommands.NoArgsCommand))!)
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
