namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class CommandSyntaxAttribute : SingleUseCommandOnlyMetadataAttribute
{
    public CommandSyntaxAttribute(params string[] syntax)
        : base(CommandMetadataTypes.SYNTAX, string.Join('\n', syntax))
    {
    }
}
