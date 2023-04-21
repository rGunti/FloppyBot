namespace FloppyBot.Commands.Core.Attributes.Metadata;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class CommandOnlyMetadataAttribute : CommandMetadataAttribute
{
    protected CommandOnlyMetadataAttribute(string type, string value)
        : base(type, value) { }
}
