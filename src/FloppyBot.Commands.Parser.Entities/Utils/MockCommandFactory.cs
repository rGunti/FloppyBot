using FloppyBot.Base.EquatableCollections;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
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

    public static ChatMessage CreateReply(
        this CommandInstruction instruction,
        string reply,
        bool sendAsReply
    )
    {
        return instruction.Context!.SourceMessage with
        {
            Content = reply,
            Identifier = sendAsReply
                ? instruction.Context!.SourceMessage.Identifier
                : ChatMessageIdentifier.NewFor(instruction.Context!.SourceMessage.Identifier),
        };
    }
}
