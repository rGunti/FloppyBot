using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class MinCommandPrivilegeAttribute : CommandOnlyMetadataAttribute
{
    public MinCommandPrivilegeAttribute(PrivilegeLevel privilegeLevel)
        : base(CommandMetadataTypes.MIN_PRIVILEGE, privilegeLevel.ToString())
    {
    }

    public PrivilegeLevel PrivilegeLevel => Enum.Parse<PrivilegeLevel>(Value);
}
