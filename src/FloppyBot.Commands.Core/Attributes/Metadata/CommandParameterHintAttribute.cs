using FloppyBot.Base.Extensions;
using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class CommandParameterHintAttribute : CommandOnlyMetadataAttribute
{
    public CommandParameterHintAttribute(
        int order,
        string paramName,
        CommandParameterType type,
        bool required = true,
        string? description = null,
        params string[] possibleValues
    )
        : base(
            CommandMetadataTypes.PARAMETER_HINTS,
            ConvertToString(order, paramName, type, required, description, possibleValues)
        )
    {
        Order = order;
        ParamName = paramName;
        Type = type;
        Required = required;
        Description = description;
        PossibleValues = possibleValues;
    }

    public int Order { get; }
    public string ParamName { get; }
    public CommandParameterType Type { get; }
    public bool Required { get; }
    public string? Description { get; }
    public string[] PossibleValues { get; }

    public string ConvertToString() =>
        ConvertToString(Order, ParamName, Type, Required, Description, PossibleValues);

    public static string ConvertToString(
        int order,
        string paramName,
        CommandParameterType type,
        bool required,
        string? description,
        IEnumerable<string> possibleValues
    )
    {
        return new[]
        {
            $"{order}",
            paramName,
            $"{type}",
            $"{(required ? 1 : 0)}",
            $"{description ?? string.Empty}",
            possibleValues.Join(";")
        }.Join("|");
    }
}
