namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class PrimaryCommandNameAttribute : SingleUseCommandOnlyMetadataAttribute
{
    public PrimaryCommandNameAttribute(string value)
        : base(CommandMetadataTypes.PRIMARY_NAME, value) { }
}
