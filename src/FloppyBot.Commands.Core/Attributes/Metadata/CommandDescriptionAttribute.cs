namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class CommandDescriptionAttribute : CommandOnlyMetadataAttribute
{
    public CommandDescriptionAttribute(string value) : base(CommandMetadataTypes.DESCRIPTION, value)
    {
    }
}
