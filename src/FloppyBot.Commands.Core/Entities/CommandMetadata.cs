using System.Collections.Immutable;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes.Metadata;

namespace FloppyBot.Commands.Core.Entities;

public class CommandMetadata
{
    public CommandMetadata()
        : this(new Dictionary<string, string>()) { }

    public CommandMetadata(IDictionary<string, string> metadata)
    {
        RawData = metadata.ToImmutableDictionary();
        Init();
    }

    public CommandMetadata(IImmutableDictionary<string, string> metadata)
    {
        RawData = metadata;
        Init();
    }

    public IImmutableDictionary<string, string> RawData { get; }

    public string this[string key] => RawData[key];

    public string? PrimaryName => RawData.GetValueOrDefault(CommandMetadataTypes.PRIMARY_NAME);
    public string? Description => RawData.GetValueOrDefault(CommandMetadataTypes.DESCRIPTION);
    public string? Category => RawData.GetValueOrDefault(CommandMetadataTypes.CATEGORY);

    public string[] Syntax { get; private set; } = Array.Empty<string>();
    public string[] AvailableOnInterfaces { get; private set; } = Array.Empty<string>();
    public PrivilegeLevel MinPrivilegeLevel { get; private set; }

    public bool HasNoParameters => RawData.ContainsKey(CommandMetadataTypes.NO_PARAMETERS);

    public CommandParameterMetadata[] Parameters { get; private set; } =
        Array.Empty<CommandParameterMetadata>();

    public bool HiddenCommand => RawData.ContainsKey(CommandMetadataTypes.HIDDEN);

    public bool HasValue(string key)
    {
        return RawData.ContainsKey(key);
    }

    public string? GetValueOrDefault(string key)
    {
        return RawData.GetValueOrDefault(key);
    }

    public Dictionary<string, string> GetRawDataAsDictionary()
    {
        return RawData.ToDictionary(i => i.Key, i => i.Value);
    }

    private void Init()
    {
        if (RawData.ContainsKey(CommandMetadataTypes.MIN_PRIVILEGE))
        {
            MinPrivilegeLevel = Enum.Parse<PrivilegeLevel>(
                RawData[CommandMetadataTypes.MIN_PRIVILEGE]
            );
        }

        if (RawData.ContainsKey(CommandMetadataTypes.INTERFACES))
        {
            AvailableOnInterfaces = RawData[CommandMetadataTypes.INTERFACES].Split(',');
        }

        if (RawData.ContainsKey(CommandMetadataTypes.SYNTAX))
        {
            Syntax = RawData[CommandMetadataTypes.SYNTAX].Split('\n');
        }

        if (RawData.ContainsKey(CommandMetadataTypes.PARAMETER_HINTS))
        {
            Parameters = RawData[CommandMetadataTypes.PARAMETER_HINTS]
                .Split("\n\n")
                .Select(CommandParameterMetadata.ParseFromString)
                .OrderBy(p => p.Order)
                .ToArray();
        }
    }
}

public record CommandParameterMetadata(
    int Order,
    string Name,
    CommandParameterType Type,
    bool Required,
    string? Description = null,
    string[]? PossibleValues = null
)
{
    public static CommandParameterMetadata ParseFromString(string inputString)
    {
        var split = inputString.Split('|');
        var possibleValues = split[5].Split(';');
        if (possibleValues.Length == 1 && possibleValues[0] == string.Empty)
        {
            possibleValues = Array.Empty<string>();
        }

        return new CommandParameterMetadata(
            int.Parse(split[0]),
            split[1],
            Enum.Parse<CommandParameterType>(split[2]),
            split[3] == "1",
            split[4] == string.Empty ? null : split[4],
            possibleValues
        );
    }

    public override string ToString()
    {
        return $"{nameof(CommandParameterMetadata)}: {Order} {Name} ({Type})";
    }
}

public enum CommandParameterType
{
    String,
    Number,
    Enum,
}
