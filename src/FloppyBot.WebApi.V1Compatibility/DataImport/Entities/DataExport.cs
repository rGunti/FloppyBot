using System.Collections.Immutable;
using FloppyBot.WebApi.V1Compatibility.Dtos;

namespace FloppyBot.WebApi.V1Compatibility.DataImport.Entities;

public record DataExport(
    string Version,
    DateTimeOffset CreatedAt,
    IImmutableList<string> ChannelIds,
    IImmutableList<QuoteDto> Quotes,
    IImmutableList<CustomCommand> CustomCommands,
    IImmutableList<SoundCommand> SoundCommands,
    IImmutableList<DataExportFile> Files,
    IImmutableList<DataExportFile> LegacyPayloads,
    IImmutableList<CounterCommandConfig> CounterCommands,
    IImmutableList<ShoutoutMessageConfig> Shoutout,
    IImmutableList<TimerMessageConfig> Timers,
    IImmutableList<CommandConfig> CommandConfigs);

