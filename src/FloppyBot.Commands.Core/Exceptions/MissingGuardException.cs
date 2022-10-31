namespace FloppyBot.Commands.Core.Exceptions;

public class MissingGuardException : Exception
{
    public MissingGuardException(Type guardAttributeType)
        : base($"Could not find a guard for the given attribute type {guardAttributeType}")
    {
        GuardAttributeAttributeType = guardAttributeType;
    }

    public Type GuardAttributeAttributeType { get; }
}
