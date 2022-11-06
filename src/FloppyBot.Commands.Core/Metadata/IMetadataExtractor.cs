using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Core.Metadata;

public interface IMetadataExtractor
{
    CommandMetadata ExtractMetadataFromCommand(CommandInfo commandInfo);
}
