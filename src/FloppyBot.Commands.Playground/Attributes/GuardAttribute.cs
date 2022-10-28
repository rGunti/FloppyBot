namespace FloppyBot.Commands.Playground.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class GuardAttribute : Attribute
{
    public GuardAttribute(Type guardType)
    {
        Type = guardType;
    }

    public Type Type { get; }
}
