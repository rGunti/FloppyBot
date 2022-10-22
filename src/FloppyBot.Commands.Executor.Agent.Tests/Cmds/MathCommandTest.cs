using FloppyBot.Base.Testing;
using FloppyBot.Chat;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Commands.Parser.Entities.Utils;
using SmartFormat;

namespace FloppyBot.Commands.Executor.Agent.Tests.Cmds;

[TestClass]
public class MathCommandTest
{
    private readonly MathCommand _command;

    public MathCommandTest()
    {
        _command = new MathCommand(
            LoggingUtils.GetLogger<MathCommand>());
    }

    [TestMethod]
    public void RespondsOnlyToKnownAliases()
    {
        foreach (var commandName in MathCommand.CommandNameSet)
        {
            Assert.IsTrue(
                _command.CanExecute(
                    MockCommandFactory.NewInstruction(
                        commandName,
                        new[] { "1+2" })));
        }

        foreach (var commandName in new[] { "some", "other", "name" })
        {
            Assert.IsFalse(
                _command.CanExecute(
                    MockCommandFactory.NewInstruction(
                        commandName,
                        new[] { "2+1" })));
        }
    }

    [DataTestMethod]
    [DataRow("1+2", "3")]
    public void CalculatesExpressionCorrectly(
        string inputStr,
        string expectedOutput)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "calc",
            new[] { inputStr });
        var reply = _command.Execute(instruction);

        Assert.IsNotNull(reply);
        Assert.AreEqual(instruction.CreateReply(MathCommand.REPLY_DEFAULT.FormatSmart(new
        {
            Answer = expectedOutput
        })), reply);
    }

    [TestMethod]
    public void RepliesWithMarkdownWhenSupported()
    {
        var instruction = MockCommandFactory.NewInstruction(
            "calc",
            new[] { "1+2*3" });
        instruction = instruction with
        {
            Context = new CommandContext(SourceMessage: instruction.Context!.SourceMessage with
            {
                SupportedFeatures = ChatInterfaceFeatures.MarkdownText
            })
        };
        var reply = _command.Execute(instruction);

        Assert.IsNotNull(reply);
        Assert.AreEqual(instruction.CreateReply(MathCommand.REPLY_MD.FormatSmart(new
        {
            Answer = "7"
        })), reply);
    }

    [DataTestMethod]
    [DataRow("1+5*(7", DisplayName = "Open Bracket")]
    public void RepliesWithErrorOnInvalidInput(string input)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "calc",
            new[] { input });
        var reply = _command.Execute(instruction);

        Assert.IsNotNull(reply);
        Assert.AreEqual(instruction.CreateReply(MathCommand.REPLY_ERR_PARSE), reply);
    }

    [DataTestMethod]
    [DataRow("1/zerp", DisplayName = "Cannot resolve symbol")]
    public void RepliesWithErrorOnErroneousInput(string input)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "calc",
            new[] { input });
        var reply = _command.Execute(instruction);

        Assert.IsNotNull(reply);
        Assert.AreEqual(instruction.CreateReply(MathCommand.REPLY_ERR_EXEC), reply);
    }
}
