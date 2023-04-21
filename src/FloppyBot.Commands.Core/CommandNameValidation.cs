using System.Text.RegularExpressions;

namespace FloppyBot.Commands.Core;

public static class CommandNameValidation
{
    private static readonly Regex CommandNameRegex = new("^[a-zA-Z0-9\\+\\-\\*]{1,}$");

    public static bool IsValidCommandName(this string? commandName)
    {
        return CommandNameRegex.IsMatch(commandName ?? string.Empty);
    }

    public static void IsValidCommandNameOrThrow(this string? commandName)
    {
        if (!commandName.IsValidCommandName())
        {
            ThrowInvalidCommandName(commandName);
        }
    }

    public static IEnumerable<string> AreAllValidCommandNamesOrThrow(
        this IEnumerable<string> commandNames
    )
    {
        foreach (var commandName in commandNames)
        {
            IsValidCommandNameOrThrow(commandName);
            yield return commandName;
        }
    }

    private static void ThrowInvalidCommandName(string? commandName)
    {
        throw new ArgumentException(
            $"{(commandName != null ? $"\"{commandName}\"" : "<null>")} is not a valid command name"
        );
    }
}
