using System.Reflection;

namespace FloppyBot.Commands.Core.Entities;

public record VariableCommandInfo : CommandInfo
{
    public VariableCommandInfo(
        string identifier,
        MethodInfo handlerMethod,
        MethodInfo testHandlerMethod
    )
        : base(Enumerable.Empty<string>(), handlerMethod)
    {
        CommandId = identifier;
        TestHandlerMethod = testHandlerMethod;
    }

    public VariableCommandInfo(string identifier, VariableCommandInfo variableCommandInfo)
        : this(
            variableCommandInfo.CommandId,
            variableCommandInfo.HandlerMethod,
            variableCommandInfo.TestHandlerMethod
        )
    {
        CommandId = identifier;
        Temporary = true;
    }

    /// <inheritdoc />
    public override string CommandId { get; }

    /// <summary>
    /// Returns the method that is used to test if this handler can handle this command.
    /// </summary>
    public MethodInfo TestHandlerMethod { get; }

    /// <inheritdoc />
    public override bool IsVariable => true;

    /// <inheritdoc />
    public override string PrimaryCommandName => CommandId;

    /// <summary>
    /// If this flag is set, this <see cref="VariableCommandInfo"/> is constructed temporarily
    /// and will be discarded after the command is run successfully
    /// </summary>
    public bool Temporary { get; }

    public override string ToString()
    {
        return $"VariableCmd {CommandId} [{(IsStatic ? "(static) " : "")}{HandlerMethod}]";
    }
}
