using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using FloppyBot.Base.Extensions;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using FloppyBot.Commands.Core.Config;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using FloppyBot.FileStorage;
using FloppyBot.WebApi.Auth.UserProfiles;
using FloppyBot.WebApi.Base.Exceptions;
using FloppyBot.WebApi.V1Compatibility.DataImport.Entities;
using FloppyBot.WebApi.V1Compatibility.Dtos;
using Microsoft.Extensions.Logging;

namespace FloppyBot.WebApi.V1Compatibility.DataImport;

internal record V1DataImport(
    IImmutableList<Tuple<string, string, string, Stream>> Files,
    IImmutableList<Quote> Quotes,
    ImmutableArray<CustomCommandDescription> CustomCommands,
    ImmutableArray<Tuple<string, string>> ShoutoutMessageSettings,
    ImmutableArray<TimerMessageConfiguration> TimerMessageConfigurations,
    ImmutableArray<CommandConfiguration> CommandConfigurations
);

public class V1DataImportService
{
    private static readonly IImmutableSet<string> SupportedVersions = ImmutableHashSet
        .Create<string>()
        .Add("2022.12")
        .Add("2022.12.0");

    private static readonly JsonSerializerOptions DefaultOptions =
        new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };

    private readonly ICommandConfigurationService _commandConfigurationService;

    private readonly ICustomCommandService _customCommandService;
    private readonly IFileService _fileService;

    private readonly ILogger<V1DataImportService> _logger;
    private readonly IMapper _mapper;
    private readonly IQuoteService _quoteService;
    private readonly IShoutoutMessageSettingService _shoutoutMessageSettingService;
    private readonly ITimerMessageConfigurationService _timerMessageConfigurationService;
    private readonly IUserService _userService;

    public V1DataImportService(
        ILogger<V1DataImportService> logger,
        IUserService userService,
        IFileService fileService,
        IMapper mapper,
        IQuoteService quoteService,
        ICustomCommandService customCommandService,
        IShoutoutMessageSettingService shoutoutMessageSettingService,
        ITimerMessageConfigurationService timerMessageConfigurationService,
        ICommandConfigurationService commandConfigurationService
    )
    {
        _logger = logger;
        _userService = userService;
        _fileService = fileService;
        _mapper = mapper;
        _quoteService = quoteService;
        _customCommandService = customCommandService;
        _shoutoutMessageSettingService = shoutoutMessageSettingService;
        _timerMessageConfigurationService = timerMessageConfigurationService;
        _commandConfigurationService = commandConfigurationService;
    }

    public bool ProcessFile(Stream fileStream, string? userId, bool simulate)
    {
        _logger.LogDebug("Trying to load file from stream ...");
        using var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        fileStream.Close();

        // Resetting seek position for stream reader
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var streamReader = new StreamReader(memoryStream);
        var json = streamReader.ReadToEnd();

        DataExport? content;
        try
        {
            content = JsonSerializer.Deserialize<DataExport>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse export payload");
            throw new BadRequestException("Provided payload invalid (Error Code N011)");
        }

        if (content == null)
        {
            _logger.LogWarning("Parsed JSON content has been read as null");
            throw new BadRequestException("Provided payload invalid (Error Code N001)");
        }

        return ProcessPayload(content, userId, simulate);
    }

    public bool ProcessPayload(DataExport payload, string? userId, bool simulate)
    {
        _logger.LogDebug("Processing payload");

        ValidateVersion(payload.Version);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            ValidateOwnership(payload.ChannelIds, userId);
        }
        else
        {
            _logger.LogWarning(
                "Running without authentication, I hope you know what this means ..."
            );
        }

        _logger.LogDebug(
            "Beginning pre-process phase, all calls are being created but none executed ..."
        );
        var importContent = PrepareImport(payload);

        // ReSharper disable once InvertIf
        if (simulate)
        {
            _logger.LogInformation("Did not import as simulate was enabled");
            return true;
        }

        return RunImport(importContent);
    }

    private static void ValidateVersion(string importVersion)
    {
        if (!SupportedVersions.Contains(importVersion))
        {
            throw new BadRequestException(
                $"Payload has been created in an unsupported version of FloppyBot: {importVersion}"
            );
        }
    }

    private static string CalculateSha256CheckSum(byte[] data)
    {
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", string.Empty);
    }

    private V1DataImport PrepareImport(DataExport payload)
    {
        var files = PrepareImportFiles(payload.Files)
            .Concat(PrepareImportFiles(payload.LegacyPayloads))
            .ToImmutableArray();
        return new V1DataImport(
            files,
            PrepareImportQuotes(payload.Quotes),
            PrepareImportCustomCommands(payload.CustomCommands)
                .Concat(PrepareImportSoundCommands(payload.SoundCommands, files))
                .ToImmutableArray(),
            PrepareImportShoutoutConfigs(payload.Shoutout),
            PrepareImportTimerMessages(payload.Timers),
            PrepareImportCommandConfigs(payload.CommandConfigs)
        );
    }

    private bool RunImport(V1DataImport import)
    {
        ImportFiles(import.Files);
        ImportQuotes(import.Quotes);
        ImportCustomCommands(import.CustomCommands);
        ImportShoutoutMessages(import.ShoutoutMessageSettings);
        ImportTimerMessageConfigurations(import.TimerMessageConfigurations);
        ImportCommandConfigurations(import.CommandConfigurations);
        return true;
    }

    private void ImportFiles(IImmutableList<Tuple<string, string, string, Stream>> filesToImport)
    {
        _logger.LogDebug("Importing {FileCount} files ...", filesToImport.Count);
        foreach (var file in filesToImport)
        {
            _logger.LogDebug(
                "Importing file {FileOwner}/{FileName} ({FileMimeType})",
                file.Item1,
                file.Item2,
                file.Item3
            );
            _fileService.CreateFile(file);
        }
    }

    private void ImportQuotes(IImmutableList<Quote> quotes)
    {
        _logger.LogDebug("Importing {QuoteCount} quotes ...", quotes.Count);
        foreach (var quote in quotes)
        {
            _logger.LogDebug(
                "Importing quote {QuoteChannel}/{QuoteId}",
                quote.ChannelMappingId,
                quote.QuoteId
            );
            _quoteService.ImportQuote(quote);
        }
    }

    private void ImportCustomCommands(IImmutableList<CustomCommandDescription> customCommands)
    {
        _logger.LogDebug(
            "Importing {CustomCommandCount} custom commands ...",
            customCommands.Count
        );
        foreach (var commandDescription in customCommands)
        {
            _logger.LogTrace(
                "Importing custom command {@CommandChannel}/{CommandName} ...",
                commandDescription.Owners,
                commandDescription.Name
            );
            _customCommandService.CreateCommand(commandDescription);
        }
    }

    private void ImportShoutoutMessages(
        IImmutableList<Tuple<string, string>> importShoutoutMessageSettings
    )
    {
        _logger.LogDebug(
            "Importing {ShoutoutConfigCount} shoutout configurations ...",
            importShoutoutMessageSettings.Count
        );
        foreach (var shoutoutMessageSetting in importShoutoutMessageSettings)
        {
            _shoutoutMessageSettingService.SetShoutoutMessage(shoutoutMessageSetting);
        }
    }

    private void ImportTimerMessageConfigurations(
        IImmutableList<TimerMessageConfiguration> importTimerMessageConfigurations
    )
    {
        _logger.LogDebug(
            "Importing {TimerMessageConfigCount} timer message configurations ...",
            importTimerMessageConfigurations.Count
        );
        foreach (TimerMessageConfiguration timerMessageConfig in importTimerMessageConfigurations)
        {
            _timerMessageConfigurationService.UpdateConfigurationForChannel(
                timerMessageConfig.Id,
                timerMessageConfig
            );
        }
    }

    private void ImportCommandConfigurations(
        IImmutableList<CommandConfiguration> importCommandConfigurations
    )
    {
        _logger.LogDebug(
            "Importing {CommandConfigCount} command configurations ...",
            importCommandConfigurations.Count
        );
        foreach (CommandConfiguration commandConfiguration in importCommandConfigurations)
        {
            _commandConfigurationService.SetCommandConfiguration(commandConfiguration);
        }
    }

    private void ValidateOwnership(IEnumerable<string> content, string? userId)
    {
        var userInfo = _userService.GetUserInfo(userId, true)!;
        var userChannels = userInfo.OwnerOf.ToHashSet();
        var payloadChannels = content.ToHashSet();

        _logger.LogTrace(
            "Checking if user has access to all channels {@PayloadChannel}; user has {@UserChannels}",
            payloadChannels,
            userChannels
        );
        if (!payloadChannels.All(channel => userChannels.Contains(channel)))
        {
            throw new UnauthorizedException(
                "You don't have permission to access at least one of the channels provided in your export file."
            );
        }
    }

    private ImmutableArray<Tuple<string, string, string, Stream>> PrepareImportFiles(
        IImmutableList<DataExportFile> files
    )
    {
        _logger.LogDebug("Preparing import of {FileCount} files ...", files.Count);

        _logger.LogDebug("Checking if enough quota is available ...");
        var channelsWithFailedClaims = files
            .GroupBy(file => file.Header.ChannelId)
            .Select(fileGroup =>
            {
                var quotaRequired = fileGroup.Sum(file => file.Header.FileSize);
                var fileCount = fileGroup.Count();
                return Tuple.Create(
                    fileGroup.Key,
                    _fileService.CanClaimStorage(fileGroup.Key, quotaRequired, fileCount)
                );
            })
            .Where(result => !result.Item2)
            .Select(result => result.Item1)
            .ToImmutableArray();

        if (channelsWithFailedClaims.Any())
        {
            throw new BadRequestException(
                $"Failed to claim enough file storage for {channelsWithFailedClaims.Length} channels: "
                    + $"{channelsWithFailedClaims.Join(", ")}"
            );
        }

        return files.Select(PrepareImportFile).ToImmutableArray();
    }

    private Tuple<string, string, string, Stream> PrepareImportFile(DataExportFile file)
    {
        _logger.LogDebug(
            "Preparing import of file {File} for {Channel}",
            file.Header.FileName,
            file.Header.ChannelId
        );

        // check file size
        if (file.Content.Length != file.Header.FileSize)
        {
            throw new BadRequestException(
                $"File size mismatch in file {file.Header.ChannelId}/{file.Header.FileName}"
            );
        }

        // check file check sum
        var calculateFileHash = CalculateSha256CheckSum(file.Content);
        if (calculateFileHash != file.CheckSum)
        {
            throw new BadRequestException(
                $"File check sum mismatch in file {file.Header.ChannelId}/{file.Header.FileName}"
            );
        }

        // check if file already exists
        if (_fileService.GetFile(file.Header.ChannelId, file.Header.FileName).HasValue)
        {
            throw new ConflictException(
                $"File with the same name already exists: {file.Header.ChannelId}/{file.Header.FileName}"
            );
        }

        // create call
        return V1DataImportServiceExtensions.CreateFileCall(
            file.Header.ChannelId,
            file.Header.FileName,
            file.Header.MimeType,
            new MemoryStream(file.Content)
        );
    }

    private ImmutableArray<Quote> PrepareImportQuotes(IImmutableList<QuoteDto> quotes)
    {
        _logger.LogDebug("Preparing import of {FileCount} quotes ...", quotes.Count);
        return quotes.Select(PrepareImportQuote).ToImmutableArray();
    }

    private Quote PrepareImportQuote(QuoteDto quote)
    {
        _logger.LogTrace("Preparing quote {QuoteChannel}/{QuoteId}", quote.Channel, quote.QuoteId);
        if (_quoteService.GetQuote(quote.Channel, quote.QuoteId) != null)
        {
            throw new BadRequestException(
                $"Quote for channel {quote.Channel} with ID {quote.QuoteId} already exists"
            );
        }

        return new Quote(
            null!,
            quote.Channel,
            quote.QuoteId,
            quote.QuoteText,
            quote.QuoteContext ?? string.Empty,
            quote.CreatedAt,
            quote.CreatedBy
        );
    }

    private IEnumerable<CustomCommandDescription> PrepareImportCustomCommands(
        IImmutableList<CustomCommand> commands
    )
    {
        _logger.LogDebug("Preparing import of {FileCount} custom commands ...", commands.Count);
        return commands.Select(PrepareImportCustomCommand);
    }

    private CustomCommandDescription PrepareImportCustomCommand(CustomCommand customCommand)
    {
        _logger.LogTrace(
            "Preparing custom command {CommandChannel}/{CommandName}",
            customCommand.Channel,
            customCommand.Command
        );

        if (_customCommandService.GetCommand(customCommand.Channel, customCommand.Command) != null)
        {
            throw new BadRequestException(
                $"Custom command {customCommand.Channel}/{customCommand.Command} already exists"
            );
        }

        return _mapper.Map<CustomCommandDescription>(customCommand);
    }

    private IEnumerable<CustomCommandDescription> PrepareImportSoundCommands(
        IImmutableList<SoundCommand> soundCommands,
        IImmutableList<Tuple<string, string, string, Stream>> knownFiles
    )
    {
        _logger.LogDebug("Preparing import of {FileCount} sound commands ...", soundCommands.Count);

        return soundCommands.Select(cmd => PrepareImportSoundCommand(cmd, knownFiles));
    }

    private CustomCommandDescription PrepareImportSoundCommand(
        SoundCommand soundCommand,
        IImmutableList<Tuple<string, string, string, Stream>> knownFiles
    )
    {
        _logger.LogTrace(
            "Preparing sound command {CommandChannel}/{CommandName}",
            soundCommand.ChannelId,
            soundCommand.CommandName
        );

        if (
            _customCommandService.GetCommand(soundCommand.ChannelId, soundCommand.CommandName)
            != null
        )
        {
            throw new BadRequestException(
                $"Custom command {soundCommand.ChannelId}/{soundCommand.CommandName} already exists"
            );
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (soundCommand.SoundFiles != null)
        {
            foreach (var soundFileRef in soundCommand.SoundFiles)
            {
                if (knownFiles.Any(file => $"{file.Item1}/{file.Item2}" == soundFileRef))
                {
                    _logger.LogTrace("{FileName} is a known file in this import", soundFileRef);
                    continue;
                }

                var channelId = soundFileRef.Split('/').Take(2).Join("/");
                var fileName = soundFileRef.Split('/').Skip(2).Join("/");
                if (_fileService.GetFile(channelId, fileName).HasValue)
                {
                    _logger.LogTrace(
                        "{FileName} is already stored and can be referenced",
                        soundFileRef
                    );
                    continue;
                }

                throw new BadRequestException(
                    $"File {soundFileRef} cannot be referenced because it doesn't exist in the upload and has not been uploaded before."
                );
            }

            return _mapper.Map<CustomCommandDescription>(soundCommand);
        }

        if (soundCommand.PayloadId != null)
        {
            _logger.LogDebug(
                "Sound command {CommandChannel}/{CommandName} is using a legacy payload, checking if that one exists",
                soundCommand.ChannelId,
                soundCommand.CommandName
            );
            if (
                !knownFiles.Any(file =>
                    file.Item1 == soundCommand.ChannelId
                    && file.Item2 == $"{soundCommand.CommandName}.wav"
                )
            )
            {
                throw new BadRequestException(
                    $"Sound command {soundCommand.ChannelId}/{soundCommand.CommandName} references a file that is not present in the import"
                );
            }

            return _mapper.Map<CustomCommandDescription>(
                soundCommand with
                {
                    SoundFiles = ImmutableList
                        .Create<string>()
                        .Add($"{soundCommand.ChannelId}/{soundCommand.CommandName}.wav"),
                }
            );
        }

        throw new BadRequestException(
            $"Sound command {soundCommand.ChannelId}/{soundCommand.CommandName} does not feature sound files or payload references"
        );
    }

    private ImmutableArray<Tuple<string, string>> PrepareImportShoutoutConfigs(
        IImmutableList<ShoutoutMessageConfig> shoutoutConfigs
    )
    {
        _logger.LogDebug(
            "Preparing import of {ShoutoutCount} shoutout commands",
            shoutoutConfigs.Count
        );
        return shoutoutConfigs.Select(PrepareImportShoutout).ToImmutableArray();
    }

    private Tuple<string, string> PrepareImportShoutout(ShoutoutMessageConfig config)
    {
        _logger.LogTrace("Preparing import of shoutout config for channel {ChannelId}", config.Id);
        return Tuple.Create(config.Id, config.Message);
    }

    private ImmutableArray<TimerMessageConfiguration> PrepareImportTimerMessages(
        IImmutableList<TimerMessageConfig> timerMessageConfigs
    )
    {
        _logger.LogDebug(
            "Preparing import of {TimerMessageCount} timer message configurations",
            timerMessageConfigs.Count
        );

        return timerMessageConfigs.Select(PrepareImportTimerMessage).ToImmutableArray();
    }

    private TimerMessageConfiguration PrepareImportTimerMessage(
        TimerMessageConfig timerMessageConfig
    )
    {
        return new TimerMessageConfiguration(
            timerMessageConfig.Id,
            timerMessageConfig.Messages,
            timerMessageConfig.Interval,
            timerMessageConfig.MinMessages
        );
    }

    private ImmutableArray<CommandConfiguration> PrepareImportCommandConfigs(
        IImmutableList<CommandConfig> commandConfigs
    )
    {
        _logger.LogDebug(
            "Preparing import of {TimerMessageCount} command configurations",
            commandConfigs.Count
        );

        return commandConfigs.Select(PrepareImportCommandConfig).ToImmutableArray();
    }

    private CommandConfiguration PrepareImportCommandConfig(CommandConfig commandConfig)
    {
        return _mapper.Map<CommandConfiguration>(commandConfig);
    }
}
