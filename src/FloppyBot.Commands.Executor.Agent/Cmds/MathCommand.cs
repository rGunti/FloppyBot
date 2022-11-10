using Expressionator.Expressions.Builder;
using Expressionator.Expressions.Evaluator;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Args;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Executor.Agent.Utils;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

[CommandHost]
[CommandCategory("Math")]
// ReSharper disable once UnusedType.Global
public class MathCommand
{
    public const string REPLY_DEFAULT = "The answer is {Answer}";
    public const string REPLY_MD = "The answer is `{Answer}`";
    public const string REPLY_ERR_PARSE = "Sorry, something in your expression doesn't make sense";
    public const string REPLY_ERR_EXEC = "Sorry, something broke while I calculated that expression";

    private readonly ILogger<MathCommand> _logger;

    public MathCommand(ILogger<MathCommand> logger)
    {
        _logger = logger;
    }

    [Command("math", "calc")]
    [PrimaryCommandName("math")]
    [CommandDescription("Calculate something using a calculator")]
    [CommandSyntax(
        "<Expression>",
        "2+3")]
    // ReSharper disable once UnusedMember.Global
    public string? CalculateMathExpression(
        CommandInstruction instruction,
        [AllArguments] string expression)
    {
        try
        {
            var result = ExpressionEvaluator.EvaluateExpression(expression);
            if (result == null)
                return null;

            return instruction.DetermineMessageTemplate(
                ChatInterfaceFeatures.MarkdownText,
                REPLY_MD,
                REPLY_DEFAULT).Format(new
            {
                Answer = result.ToString()
            });
        }
        catch (UnexpectedTokenException ex)
        {
            _logger.LogWarning(ex, "Failed to parse expression {Expression}", expression);
            return REPLY_ERR_PARSE;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute expression {Expression}", expression);
            return REPLY_ERR_EXEC;
        }
    }
}
