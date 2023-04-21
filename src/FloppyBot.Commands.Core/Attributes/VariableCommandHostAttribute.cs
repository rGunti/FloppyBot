namespace FloppyBot.Commands.Core.Attributes;

/// <summary>
/// Classes marked with this attribute are hosting
/// commands that are not hardcoded but delivered dynamically from
/// another data source, like a database.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class VariableCommandHostAttribute : Attribute { }

/// <summary>
/// Methods marked with this attribute are supposed to execute
/// variable commands.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class VariableCommandHandlerAttribute : Attribute
{
    public VariableCommandHandlerAttribute(string assertionHandlerName, string? identifier = null)
    {
        Identifier = identifier;
        AssertionHandlerName = assertionHandlerName;
    }

    public string AssertionHandlerName { get; }
    public string? Identifier { get; }
}
