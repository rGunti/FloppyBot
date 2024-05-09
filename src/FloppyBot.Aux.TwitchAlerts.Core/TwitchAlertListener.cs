using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;
using FloppyBot.Aux.TwitchAlerts.Core.Entities;
using FloppyBot.Base.Configuration;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.TextFormatting;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Aux.TwitchAlerts.Core;

public class TwitchAlertListener : IDisposable
{
    private static readonly FrozenSet<string> AllowedEvents = new[]
    {
        TwitchEventTypes.SUBSCRIPTION,
        TwitchEventTypes.RE_SUBSCRIPTION,
        TwitchEventTypes.SUBSCRIPTION_GIFT,
        TwitchEventTypes.SUBSCRIPTION_GIFT_COMMUNITY,
    }.ToFrozenSet();

    private static readonly IImmutableDictionary<
        TwitchSubscriptionPlanTier,
        Func<TwitchAlertMessage, string>
    > MessageTemplateSelectors = new Dictionary<
        TwitchSubscriptionPlanTier,
        Func<TwitchAlertMessage, string>
    >
    {
        { TwitchSubscriptionPlanTier.Tier1, m => m.Tier1Message ?? m.DefaultMessage },
        { TwitchSubscriptionPlanTier.Tier2, m => m.Tier2Message ?? m.DefaultMessage },
        { TwitchSubscriptionPlanTier.Tier3, m => m.Tier3Message ?? m.DefaultMessage },
        { TwitchSubscriptionPlanTier.Prime, m => m.PrimeMessage ?? m.DefaultMessage },
    }.ToImmutableDictionary();

    private static readonly Func<TwitchAlertMessage, string> DefaultTemplateSelector = m =>
        m.DefaultMessage;

    private static readonly IImmutableDictionary<
        string,
        Func<TwitchAlertSettings, IEnumerable<TwitchAlertMessage>>
    > MessageTemplateListSelector = new Dictionary<
        string,
        Func<TwitchAlertSettings, IEnumerable<TwitchAlertMessage>>
    >
    {
        { TwitchEventTypes.SUBSCRIPTION, s => s.SubMessage },
        { TwitchEventTypes.RE_SUBSCRIPTION, s => s.ReSubMessage },
        { TwitchEventTypes.SUBSCRIPTION_GIFT, s => s.GiftSubMessage },
        { TwitchEventTypes.SUBSCRIPTION_GIFT_COMMUNITY, s => s.GiftSubCommunityMessage },
        { TwitchEventTypes.RAID, s => s.RaidAlertMessage },
    }.ToImmutableDictionary();

    private static string? DetermineTemplate(
        TwitchEvent twitchEvent,
        TwitchAlertSettings alertSettings,
        TwitchSubscriptionPlanTier subTier
    )
    {
        var templateListSelector = MessageTemplateListSelector.GetValueOrDefault(
            twitchEvent.EventName,
            _ => Enumerable.Empty<TwitchAlertMessage>()
        );
        var templateSelector = MessageTemplateSelectors.GetValueOrDefault(
            subTier,
            DefaultTemplateSelector
        );

        var templates = templateListSelector(alertSettings).Select(templateSelector).ToList();
        // TODO: Get a random one
        return templates.FirstOrDefault();
    }

    private static TwitchSubscriptionPlanTier GetTier(TwitchEvent twitchEvent)
    {
        return twitchEvent switch
        {
            TwitchSubscriptionReceivedEvent subReceived => subReceived.SubscriptionPlanTier.Tier,
            TwitchReSubscriptionReceivedEvent reSubscriptionReceivedEvent
                => reSubscriptionReceivedEvent.SubscriptionPlanTier.Tier,
            TwitchSubscriptionGiftEvent subGiftEvent => subGiftEvent.SubscriptionPlanTier.Tier,
            TwitchSubscriptionCommunityGiftEvent subCommunityGiftEvent
                => subCommunityGiftEvent.SubscriptionPlanTier.Tier,
            TwitchRaidEvent _ => TwitchSubscriptionPlanTier.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(twitchEvent)),
        };
    }

    private static TwitchEvent? ParseTwitchEvent(string type, string content)
    {
        return type switch
        {
            TwitchEventTypes.SUBSCRIPTION
                => JsonSerializer.Deserialize<TwitchSubscriptionReceivedEvent>(content),
            TwitchEventTypes.RE_SUBSCRIPTION
                => JsonSerializer.Deserialize<TwitchReSubscriptionReceivedEvent>(content),
            TwitchEventTypes.SUBSCRIPTION_GIFT
                => JsonSerializer.Deserialize<TwitchSubscriptionGiftEvent>(content),
            TwitchEventTypes.SUBSCRIPTION_GIFT_COMMUNITY
                => JsonSerializer.Deserialize<TwitchSubscriptionCommunityGiftEvent>(content),
            TwitchEventTypes.RAID => JsonSerializer.Deserialize<TwitchRaidEvent>(content),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    private readonly ILogger<TwitchAlertListener> _logger;
    private readonly INotificationReceiver<ChatMessage> _chatMessageReceiver;
    private readonly INotificationSender _responder;
    private readonly ITwitchAlertService _alertService;

    public TwitchAlertListener(
        ILogger<TwitchAlertListener> logger,
        INotificationReceiverFactory receiverFactor,
        INotificationSenderFactory senderFactory,
        IConfiguration configuration,
        ITwitchAlertService alertService
    )
    {
        _logger = logger;
        _alertService = alertService;
        _chatMessageReceiver = receiverFactor.GetNewReceiver<ChatMessage>(
            configuration.GetParsedConnectionString("MessageInput")
        );
        _responder = senderFactory.GetNewSender(
            configuration.GetParsedConnectionString("MessageOutput")
        );

        _chatMessageReceiver.NotificationReceived += OnMessageReceived;
    }

    public void Start()
    {
        _logger.LogInformation(
            "Connecting to message input to start listening for incoming messages"
        );
        _chatMessageReceiver.StartListening();
    }

    public void Stop()
    {
        _logger.LogInformation("Shutting down Twitch Alert Listener");
        _chatMessageReceiver.StopListening();
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }

    private void OnMessageReceived(ChatMessage chatMessage)
    {
        if (!AllowedEvents.Contains(chatMessage.EventName))
        {
#if DEBUG
            _logger.LogDebug(
                "Received chat message with event name that is not allowed: {ChatMessageEventName}",
                chatMessage.EventName
            );
#endif
            return;
        }
#if DEBUG
        _logger.LogInformation("Received chat message to count: {@ChatMessage}", chatMessage);
#endif

        var twitchEvent = ParseTwitchEvent(chatMessage.EventName, chatMessage.Content);
        if (twitchEvent is null)
        {
            _logger.LogWarning("Failed to parse chat message content");
            return;
        }

        var alertMessage = GetFormattedMessage(chatMessage, twitchEvent);
        if (alertMessage is null)
        {
            return;
        }

        var response = chatMessage with
        {
            Content = alertMessage.Format(
                twitchEvent.AsDictionary().Add("User", chatMessage.Author.DisplayName)
            ),
        };

        _responder.Send(response);
    }

    private string? GetFormattedMessage(ChatMessage chatMessage, TwitchEvent twitchEvent)
    {
        var channelId = chatMessage.Identifier.GetChannel();
        var alertSettings = _alertService.GetAlertSettings(channelId);
        if (alertSettings is null)
        {
            _logger.LogDebug("No alert settings found for channel {Channel}, skipping", channelId);
            return null;
        }

        var subTier = GetTier(twitchEvent);

        var template = DetermineTemplate(twitchEvent, alertSettings, subTier);
        if (template is null)
        {
            _logger.LogDebug(
                "No template found for event {EventName} and tier {Tier}, skipping",
                twitchEvent.EventName,
                subTier
            );
            return null;
        }

        return template.Format(twitchEvent);
    }
}
