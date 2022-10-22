using System.Collections.Immutable;
using FloppyBot.Base.Testing;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;

namespace FloppyBot.Commands.Executor.Agent.Tests.Cmds;

[TestClass]
public class PingCommandTest
{
    private readonly PingCommand _pingCommand;

    public PingCommandTest()
    {
        _pingCommand = new PingCommand(
            LoggingUtils.GetLogger<PingCommand>());
    }

    [TestMethod]
    public void RespondsToPing()
    {
        Assert.IsTrue(_pingCommand.CanExecute(new CommandInstruction(
            "ping",
            ImmutableArray<string>.Empty)));
        Assert.IsFalse(_pingCommand.CanExecute(new CommandInstruction(
            "pong",
            ImmutableArray<string>.Empty)));
    }

    [TestMethod]
    public void RepliesWithPong()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "ping",
            Array.Empty<string>());
        var reply = _pingCommand.Execute(instruction);

        Assert.IsNotNull(reply);
        Assert.AreEqual(instruction.CreateReply("Pong!"), reply);
    }
}
