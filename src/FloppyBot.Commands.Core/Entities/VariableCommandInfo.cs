using System.Reflection;

namespace FloppyBot.Commands.Core.Entities;

public record VariableCommandInfo : CommandInfo
{
    public VariableCommandInfo(
        string identifier,
        MethodInfo handlerMethod,
        MethodInfo testHandlerMethod)
        : base(Enumerable.Empty<string>(), handlerMethod)
    {
        Identifier = identifier;
        TestHandlerMethod = testHandlerMethod;
    }

    /// <summary>
    /// Returns an identifier for this variable command handler
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Returns the method that is used to test if this handler can handle this command.
    /// </summary>
    public MethodInfo TestHandlerMethod { get; }

    /// <inheritdoc />
    public override bool IsVariable => true;

    public override string ToString()
    {
        return $"VariableCmd {Identifier} [{(IsStatic ? "(static) " : "")}{HandlerMethod}]";
    }
}
