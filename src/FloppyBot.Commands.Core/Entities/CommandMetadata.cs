using System.Collections.Immutable;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Core.Attributes.Metadata;

namespace FloppyBot.Commands.Core.Entities;

public class CommandMetadata
{
    public CommandMetadata() : this(new Dictionary<string, string>())
    {
    }

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

    private void Init()
    {
        if (RawData.ContainsKey(CommandMetadataTypes.MIN_PRIVILEGE))
        {
            MinPrivilegeLevel = Enum.Parse<PrivilegeLevel>(RawData[CommandMetadataTypes.MIN_PRIVILEGE]);
        }

        if (RawData.ContainsKey(CommandMetadataTypes.INTERFACES))
        {
            AvailableOnInterfaces = RawData[CommandMetadataTypes.INTERFACES].Split(',');
        }

        if (RawData.ContainsKey(CommandMetadataTypes.SYNTAX))
        {
            Syntax = RawData[CommandMetadataTypes.SYNTAX].Split('\n');
        }
    }

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
}
