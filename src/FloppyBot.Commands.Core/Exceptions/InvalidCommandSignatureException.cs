using System.Reflection;

namespace FloppyBot.Commands.Core.Exceptions;

public class InvalidCommandSignatureException : Exception
{
    private const string MESSAGE =
        "The provided method \"{0}\" does not have a supported signature.";

    public InvalidCommandSignatureException(MethodInfo methodInfo)
        : base(string.Format(MESSAGE, methodInfo))
    {
        MethodInfo = methodInfo;
    }

    public MethodInfo MethodInfo { get; }
}
