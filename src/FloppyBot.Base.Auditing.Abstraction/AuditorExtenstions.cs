using System.Diagnostics;
using FloppyBot.Base.Auditing.Abstraction.Entities;
using FloppyBot.Chat.Entities.Identifiers;

namespace FloppyBot.Base.Auditing.Abstraction;

[StackTraceHidden]
public static class AuditorExtensions
{
    /// <summary>
    /// Records a new event
    /// </summary>
    /// <seealso cref="IAuditor.Record"/>
    /// <param name="auditor">The auditor to use</param>
    /// <param name="userIdentifier">The identifier of the user taking the action</param>
    /// <param name="channelIdentifier">The channel that this action was taken on</param>
    /// <param name="objectType">The type of the object subjected to change</param>
    /// <param name="objectIdentifier">An identifier for the object</param>
    /// <param name="action">The action performed on the object</param>
    /// <param name="additionalData">Additional context data</param>
    /// <param name="timestamp">Optional timestamp</param>
    public static void Record(
        this IAuditor auditor,
        string userIdentifier,
        string channelIdentifier,
        string objectType,
        string objectIdentifier,
        string action,
        string? additionalData = null,
        DateTimeOffset? timestamp = null
    )
    {
        auditor.Record(
            new AuditRecord(
                // The ID is set to null here, but it will be set by the storage layer
                Id: null!,
                Timestamp: timestamp ?? DateTimeOffset.MinValue,
                UserIdentifier: userIdentifier,
                ChannelIdentifier: channelIdentifier,
                ObjectType: objectType,
                ObjectIdentifier: objectIdentifier,
                Action: action,
                AdditionalData: additionalData
            )
        );
    }

    /// <summary>
    /// Records a new event
    /// </summary>
    /// <param name="auditor">The auditor to use</param>
    /// <param name="user">The identifier of the user taking the action</param>
    /// <param name="channel">The channel that this action was taken on</param>
    /// <param name="objectType">The type of the object subjected to change</param>
    /// <param name="objectIdentifier">An identifier for the object</param>
    /// <param name="action">The action performed on the object</param>
    /// <param name="additionalData">Additional context data</param>
    /// <param name="timestamp">Optional timestamp</param>
    public static void Record(
        this IAuditor auditor,
        ChannelIdentifier user,
        ChannelIdentifier channel,
        string objectType,
        string objectIdentifier,
        string action,
        string? additionalData = null,
        DateTimeOffset? timestamp = null
    )
    {
        auditor.Record(
            user.ToString(),
            channel.ToString(),
            objectType,
            objectIdentifier,
            action,
            additionalData,
            timestamp
        );
    }

    /// <summary>
    /// Records a new event
    /// </summary>
    /// <param name="auditor">The auditor to use</param>
    /// <param name="user">The identifier of the user taking the action</param>
    /// <param name="channel">The channel that this action was taken on</param>
    /// <param name="object">The object affected</param>
    /// <param name="objectIdentifier">A function that returns the identifier of the object</param>
    /// <param name="action">The action performed on the object</param>
    /// <param name="additionalData">Additional context data</param>
    /// <param name="timestamp">Optional timestamp</param>
    public static void Record<T>(
        this IAuditor auditor,
        ChannelIdentifier user,
        ChannelIdentifier channel,
        T @object,
        Func<T, string> objectIdentifier,
        string action,
        string? additionalData = null,
        DateTimeOffset? timestamp = null
    )
    {
        auditor.Record(
            user.ToString(),
            channel.ToString(),
            typeof(T).Name,
            objectIdentifier(@object),
            action,
            additionalData,
            timestamp
        );
    }
}
