using FloppyBot.Chat.Entities;

namespace FloppyBot.Commands.Core.Attributes.Metadata;

public class MinCommandPrivilegeAttribute : CommandOnlyMetadataAttribute
{
    public MinCommandPrivilegeAttribute(PrivilegeLevel privilegeLevel)
        : base(CommandMetadataTypes.MIN_PRIVILEGE, privilegeLevel.ToString())
    {
        PrivilegeLevel = privilegeLevel;
    }

    public PrivilegeLevel PrivilegeLevel { get; }
    public override string Value => PrivilegeLevel.ToString();
}
