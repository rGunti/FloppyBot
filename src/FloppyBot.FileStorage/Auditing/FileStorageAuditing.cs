using System.Diagnostics;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.FileStorage.Auditing;

[StackTraceHidden]
public static class FileStorageAuditing
{
    public const string FileHeaderType = "File";

    public static void FileCreated(
        this IAuditor auditor,
        ChatUser author,
        ChannelIdentifier channel,
        string filename,
        long fileSize
    )
    {
        auditor.Record(
            author.Identifier,
            channel,
            FileHeaderType,
            filename,
            CommonActions.Created,
            $"{fileSize} bytes"
        );
    }

    public static void FileDeleted(
        this IAuditor auditor,
        ChatUser author,
        ChannelIdentifier channel,
        string filename
    )
    {
        auditor.Record(author.Identifier, channel, FileHeaderType, filename, CommonActions.Deleted);
    }
}
