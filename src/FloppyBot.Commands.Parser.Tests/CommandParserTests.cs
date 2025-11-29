using FloppyBot.Base.EquatableCollections;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FloppyBot.Commands.Parser.Tests;

[TestClass]
public class CommandParserTests
{
    private readonly CommandParser _parser = new("!");

    [TestMethod]
    [DataRow("!ping", "ping")]
    [DataRow("!say Hello World", "say", "Hello", "World")]
    public void ParsesCommandsCorrectly(
        string input,
        string expectedCommand,
        params string[] arguments
    )
    {
        var expected = new CommandInstruction(
            expectedCommand,
            arguments.ToImmutableListWithValueSemantics()
        );

        Assert.AreEqual(expected, _parser.ParseCommandFromString(input));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("hello world")]
    [DataRow("?notacommand")]
    public void DoesNotParseCommandsWithoutPrefix(string input)
    {
        Assert.IsNull(_parser.ParseCommandFromString(input));
    }
}
