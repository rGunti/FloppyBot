using System.Collections.Immutable;

namespace FloppyBot.Commands.Core.Attributes;

/// <summary>
/// This attribute denotes methods that implement a command handler.
/// The attribute requires at least one string containing the command name.
/// If any command name is considered invalid, an <see cref="ArgumentException"/>
/// will be thrown.
/// </summary>
/// <seealso cref="CommandNameValidation.IsValidCommandName"/>
[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public CommandAttribute(string name, params string[] aliases)
    {
        Names = aliases.Prepend(name)
            .AreAllValidCommandNamesOrThrow()
            .ToImmutableHashSet();
    }

    /// <summary>
    /// A set of command names used to call this command
    /// </summary>
    public IImmutableSet<string> Names { get; }
}
