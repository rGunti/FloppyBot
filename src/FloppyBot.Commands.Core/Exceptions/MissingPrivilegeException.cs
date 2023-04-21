using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Exceptions;

public class MissingPrivilegeException : Exception
{
    private const string EXCEPTION_MESSAGE =
        "Required privilege level not met; expected at least {Expected}, was {Actual}";

    public MissingPrivilegeException(PrivilegeLevel expectedLevel, PrivilegeLevel actualLevel)
        : base(EXCEPTION_MESSAGE.Format(new { Expected = expectedLevel, Actual = actualLevel })) { }
}

public static class MissingPrivilegeExceptionExtensions
{
    public static void AssertLevel(this ChatUser user, PrivilegeLevel expectedLevel)
    {
        user.PrivilegeLevel.AssertLevel(expectedLevel);
    }

    public static void AssertLevel(this PrivilegeLevel actualLevel, PrivilegeLevel expectedLevel)
    {
        if (expectedLevel > actualLevel)
        {
            throw new MissingPrivilegeException(expectedLevel, actualLevel);
        }
    }
}
