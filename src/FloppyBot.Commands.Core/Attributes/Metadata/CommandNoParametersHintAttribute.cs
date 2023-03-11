namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class CommandNoParametersHintAttribute : SingleUseCommandOnlyMetadataAttribute
{
    public CommandNoParametersHintAttribute() : base(CommandMetadataTypes.NO_PARAMETERS, "1")
    {
    }
}