namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class CommandAvailableInterfaceAttribute : SingleUseCommandOnlyMetadataAttribute
{
    public CommandAvailableInterfaceAttribute(params string[] messageInterfaces) : base(CommandMetadataTypes.INTERFACES,
        string.Join(",", messageInterfaces))
    {
        Interfaces = messageInterfaces;
    }

    public string[] Interfaces { get; }
}
