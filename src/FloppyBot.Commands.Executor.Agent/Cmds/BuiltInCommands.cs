using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Executor.Agent.Utils;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Version;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

[CommandHost]
// ReSharper disable once UnusedType.Global
public class BuiltInCommands
{
    public const string REPLY_PING = "Pong!";
    private const string REPLY_ABOUT_DEFAULT = "Hello! This is FloppyBot v{Version}!";
    private const string REPLY_ABOUT_MD = "Hello! This is **FloppyBot v{Version}**";

    [Command("ping")]
    public static string Ping() => REPLY_PING;

    [Command("about")]
    public static string About(
        CommandInstruction instruction)
    {
        var template = instruction.SourceSupports(ChatInterfaceFeatures.MarkdownText)
            ? REPLY_ABOUT_MD
            : REPLY_ABOUT_DEFAULT;
        return template.Format(AboutThisApp.Info);
    }
}
