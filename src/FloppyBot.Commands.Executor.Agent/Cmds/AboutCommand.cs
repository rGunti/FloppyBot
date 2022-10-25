using System.Collections.Immutable;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Executor.Agent.Utils;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Version;
using Microsoft.Extensions.Logging;
using SmartFormat;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

// ReSharper disable once ClassNeverInstantiated.Global
public class AboutCommand : RegularBotCommand
{
    private const string REPLY_DEFAULT = "Hello! This is FloppyBot v{Version}!";
    private const string REPLY_MD = "Hello! This is **FloppyBot v{Version}**";

    private static readonly IImmutableSet<string> CommandNameSet = new[] { "about" }.ToImmutableHashSet();

    private readonly ILogger<AboutCommand> _logger;

    public AboutCommand(ILogger<AboutCommand> logger)
    {
        _logger = logger;
    }

    protected override IImmutableSet<string> CommandNames => CommandNameSet;

    public override ChatMessage? Execute(CommandInstruction instruction)
    {
        return instruction.ReplyIfNotEmpty(
            (instruction.SourceSupports(ChatInterfaceFeatures.MarkdownText) ? REPLY_MD : REPLY_DEFAULT).FormatSmart(
                AboutThisApp.Info));
    }
}
