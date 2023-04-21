using FloppyBot.Base.EquatableCollections;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Mock;

namespace FloppyBot.Commands.Parser.Entities.Utils;

public static class MockCommandFactory
{
    public static CommandInstruction NewInstruction(
        string commandName,
        string[] commandArgs,
        PrivilegeLevel privilegeLevel = PrivilegeLevel.Unknown
    )
    {
        return new CommandInstruction(
            commandName,
            commandArgs.ToImmutableListWithValueSemantics(),
            new CommandContext(
                MockMessageFactory.NewChatMessage(
                    $"{commandName} {string.Join(' ', commandArgs)}",
                    user: MockMessageFactory.NewChatUser("MockUser", "Mock User", privilegeLevel)
                )
            )
        );
    }

    public static ChatMessage CreateReply(this CommandInstruction instruction, string reply)
    {
        return instruction.Context!.SourceMessage with { Content = reply };
    }
}
