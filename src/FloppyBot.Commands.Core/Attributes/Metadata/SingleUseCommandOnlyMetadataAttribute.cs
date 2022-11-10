namespace FloppyBot.Commands.Core.Attributes.Metadata;

[AttributeUsage(AttributeTargets.Method)]
public abstract class SingleUseCommandOnlyMetadataAttribute : CommandOnlyMetadataAttribute
{
    protected SingleUseCommandOnlyMetadataAttribute(string type, string value) : base(type, value)
    {
    }
}
