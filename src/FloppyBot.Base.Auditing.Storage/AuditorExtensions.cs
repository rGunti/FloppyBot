using System.Diagnostics;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Storage;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Base.Auditing.Storage;

public static class AuditorExtensions
{
    /// <summary>
    /// Records a new event
    /// </summary>
    /// <param name="auditor">The auditor to use</param>
    /// <param name="user">The identifier of the user taking the action</param>
    /// <param name="channel">The channel that this action was taken on</param>
    /// <param name="object">The object affected</param>
    /// <param name="action">The action performed on the object</param>
    /// <param name="additionalData">Additional context data</param>
    /// <param name="timestamp">Optional timestamp</param>
    [StackTraceHidden]
    public static void Record<T>(
        this IAuditor auditor,
        ChannelIdentifier user,
        ChannelIdentifier channel,
        T @object,
        string action,
        string? additionalData = null,
        DateTimeOffset? timestamp = null
    )
        where T : IEntity<T>
    {
        auditor.Record(
            user,
            channel,
            @object,
            GetObjectIdentifier,
            action,
            additionalData ?? @object.ToString()
        );
    }

    private static string GetObjectIdentifier<T>(T @object)
        where T : IEntity<T>
    {
        return @object.Id;
    }
}
