using System.Collections.Immutable;

namespace FloppyBot.Commands.Core.Attributes.Guards;

public class SourceInterfaceGuardAttribute : GuardAttribute
{
    public SourceInterfaceGuardAttribute(params string[] allowedMessageInterfaces)
    {
        AllowedMessageInterfaces = allowedMessageInterfaces.ToImmutableHashSet();
    }

    public IImmutableSet<string> AllowedMessageInterfaces { get; }

    public override string ToString()
    {
        return $"{nameof(SourceInterfaceGuardAttribute)}({string.Join(", ", AllowedMessageInterfaces)})";
    }
}
