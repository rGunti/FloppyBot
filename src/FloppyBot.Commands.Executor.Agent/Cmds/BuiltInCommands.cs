using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes;
using FloppyBot.Commands.Core.Attributes.Metadata;
using FloppyBot.Commands.Executor.Agent.Utils;
using FloppyBot.Commands.Parser.Entities;
using FloppyBot.Version;

namespace FloppyBot.Commands.Executor.Agent.Cmds;

[CommandHost]
[CommandCategory("Diagnostics")]
// ReSharper disable once UnusedType.Global
public class BuiltInCommands
{
    public const string REPLY_PING = "Pong!";
    private const string REPLY_ABOUT_DEFAULT = "Hello! This is FloppyBot v{Version}!";
    private const string REPLY_ABOUT_MD = "Hello! This is **FloppyBot v{Version}**";

    [Command("ping")]
    [PrimaryCommandName("ping")]
    [CommandDescription(
        "Returns a test message. Useful to check if FloppyBot responds to commands."
    )]
    [CommandCooldown(PrivilegeLevel.Viewer, 30000)]
    [CommandNoParametersHint]
    public static string Ping() => REPLY_PING;

    [Command("about")]
    [PrimaryCommandName("about")]
    [CommandDescription("Returns FloppyBots current version")]
    [CommandCooldown(PrivilegeLevel.Viewer, 30000)]
    [CommandNoParametersHint]
    public static string About(CommandInstruction instruction)
    {
        var template = instruction.SourceSupports(ChatInterfaceFeatures.MarkdownText)
            ? REPLY_ABOUT_MD
            : REPLY_ABOUT_DEFAULT;
        return template.Format(AboutThisApp.Info);
    }
}
