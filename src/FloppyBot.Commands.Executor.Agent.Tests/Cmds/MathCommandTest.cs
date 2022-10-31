using FloppyBot.Base.Testing;
using FloppyBot.Base.TextFormatting;
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

    [DataTestMethod]
    [DataRow("1+2", "3")]
    public void CalculatesExpressionCorrectly(
        string inputStr,
        string expectedOutput)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "calc",
            new[] { inputStr });
        var reply = _command.CalculateMathExpression(
            instruction,
            inputStr);

        Assert.IsNotNull(reply);
        Assert.AreEqual(MathCommand.REPLY_DEFAULT.Format(new
        {
            Answer = expectedOutput
        }), reply);
    }

    [DataTestMethod]
    [DataRow("1+2", "3")]
    public void RepliesWithMarkdownWhenSupported(
        string inputStr,
        string expectedOutput)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "calc",
            new[] { inputStr });
        instruction = instruction with
        {
            Context = new CommandContext(SourceMessage: instruction.Context!.SourceMessage with
            {
                SupportedFeatures = ChatInterfaceFeatures.MarkdownText
            })
        };
        var reply = _command.CalculateMathExpression(instruction, inputStr);

        Assert.IsNotNull(reply);
        Assert.AreEqual(MathCommand.REPLY_MD.FormatSmart(new
        {
            Answer = expectedOutput
        }), reply);
    }

    [DataTestMethod]
    [DataRow("1+5*(7", DisplayName = "Open Bracket")]
    public void RepliesWithErrorOnInvalidInput(string input)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "calc",
            new[] { input });
        var reply = _command.CalculateMathExpression(instruction, input);

        Assert.IsNotNull(reply);
        Assert.AreEqual(MathCommand.REPLY_ERR_PARSE, reply);
    }

    [DataTestMethod]
    [DataRow("1/zerp", DisplayName = "Cannot resolve symbol")]
    public void RepliesWithErrorOnErroneousInput(string input)
    {
        var instruction = MockCommandFactory.NewInstruction(
            "calc",
            new[] { input });
        var reply = _command.CalculateMathExpression(instruction, input);

        Assert.IsNotNull(reply);
        Assert.AreEqual(MathCommand.REPLY_ERR_EXEC, reply);
    }
}
