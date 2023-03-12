namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class CommandHiddenAttribute : SingleUseCommandOnlyMetadataAttribute
{
    public CommandHiddenAttribute() : base(CommandMetadataTypes.HIDDEN, "1")
    {
    }
}
