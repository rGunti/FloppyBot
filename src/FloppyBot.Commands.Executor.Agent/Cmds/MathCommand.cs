using System.Collections.Immutable;
using Expressionator.Expressions.Builder;
using Expressionator.Expressions.Evaluator;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Executor.Agent.Utils;
using FloppyBot.Commands.Parser.Entities;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

// ReSharper disable once ClassNeverInstantiated.Global
public class MathCommand : RegularBotCommand
{
    public const string REPLY_DEFAULT = "The answer is {Answer}";
    public const string REPLY_MD = "The answer is `{Answer}`";
    public const string REPLY_ERR_PARSE = "Sorry, something in your expression doesn't make sense";
    public const string REPLY_ERR_EXEC = "Sorry, something broke while I calculated that expression";

    public static readonly IImmutableSet<string> CommandNameSet = new[] { "math", "calc" }.ToImmutableHashSet();

    private readonly ILogger<MathCommand> _logger;

    public MathCommand(ILogger<MathCommand> logger)
    {
        _logger = logger;
    }

    protected override IImmutableSet<string> CommandNames => CommandNameSet;

    public override bool CanExecute(CommandInstruction instruction)
    {
        return base.CanExecute(instruction) && instruction.Parameters.Any();
    }

    public override ChatMessage? Execute(CommandInstruction instruction)
    {
        string? reply;
        var expression = instruction.Parameters.JoinToString();
        try
        {
            var result = ExpressionEvaluator.EvaluateExpression(expression);
            if (result == null)
                return null;

            reply = (instruction.SourceSupports(ChatInterfaceFeatures.MarkdownText) ? REPLY_MD : REPLY_DEFAULT)
                .FormatSmart(new
                {
                    Answer = result.ToString()
                });
        }
        catch (UnexpectedTokenException ex)
        {
            _logger.LogWarning(ex, "Failed to parse expression {Expression}", expression);
            reply = REPLY_ERR_PARSE;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to execute expression {Expression}", expression);
            reply = REPLY_ERR_EXEC;
        }

        return instruction.ReplyIfNotEmpty(reply);
    }
}
