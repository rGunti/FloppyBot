namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class CommandCategoryAttribute : CommandMetadataAttribute
{
    public CommandCategoryAttribute(string value) : base(CommandMetadataTypes.CATEGORY, value)
    {
    }
}
