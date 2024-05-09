using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using FakeItEasy;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Chat.Twitch.Api;
using FloppyBot.Chat.Twitch.Config;
using FloppyBot.Chat.Twitch.Events;
using FloppyBot.Chat.Twitch.Monitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using ChatMessage = FloppyBot.Chat.Entities.ChatMessage;

namespace FloppyBot.Chat.Twitch.Tests;

[TestClass]
public class TwitchChatInterfaceTests
{
    private readonly ITwitchClient _client = A.Fake<ITwitchClient>();
    private readonly ITwitchChannelOnlineMonitor _onlineMonitor =
        A.Fake<ITwitchChannelOnlineMonitor>();

    private TwitchConfiguration _configuration =
        new(
            "atwitchbot",
            "sometoken",
            "atwitchchannel",
            "aclientid",
            "anaccesstoken",
            false,
            0,
            false
        );

    [TestMethod]
    public void BroadcasterHasAdminRights()
    {
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world", isBroadcaster: true),
            PrivilegeLevel.Administrator
        );
    }

    [TestMethod]
    public void ModeratorHasModRights()
    {
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world", isModerator: true),
            PrivilegeLevel.Moderator
        );
    }

    [TestMethod]
    public void ViewerHasViewerRights()
    {
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world"),
            PrivilegeLevel.Viewer
        );
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world", isPartner: true),
            PrivilegeLevel.Viewer
        );
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world", isStaff: true),
            PrivilegeLevel.Viewer
        );
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world", isSubscriber: true),
            PrivilegeLevel.Viewer
        );
        CheckPrivilegeLevel(
            CreateChatMessage(
                "atwitchviewer",
                "hello world",
                isPartner: true,
                isStaff: true,
                isSubscriber: true
            ),
            PrivilegeLevel.Viewer
        );
    }

    [TestMethod]
    public void BotUserHasNoRights()
    {
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world", isMe: true),
            PrivilegeLevel.Unknown
        );
    }

    [TestMethod]
    public void MessageIsSent()
    {
        TwitchChatInterface chatInterface = CreateInterface();

        chatInterface.SendMessage("A message\n\nwith multiple lines");

        A.CallTo(
                () =>
                    _client.SendMessage(
                        A<string>.That.IsEqualTo(_configuration.Channel),
                        A<string>.That.IsEqualTo("A message"),
                        A<bool>.Ignored
                    )
            )
            .MustHaveHappenedOnceExactly();
        A.CallTo(
                () =>
                    _client.SendMessage(
                        A<string>.That.IsEqualTo(_configuration.Channel),
                        A<string>.That.IsEqualTo("with multiple lines"),
                        A<bool>.Ignored
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void MessageIsNotEmittedIfOffline()
    {
        TwitchChatInterface chatInterface = CreateInterface(
            _configuration with
            {
                DisableWhenChannelIsOffline = true,
            }
        );
        var receivedMessages = 0;
        chatInterface.MessageReceived += (_, _) => receivedMessages++;

        A.CallTo(() => _onlineMonitor.IsChannelOnline()).Returns(false);

        _client.OnMessageReceived += Raise.With(CreateChatMessage("atwitchviewer", "hello world"));
        Assert.AreEqual(0, receivedMessages);

        A.CallTo(() => _onlineMonitor.IsChannelOnline()).Returns(true);

        _client.OnMessageReceived += Raise.With(CreateChatMessage("atwitchviewer", "hello world"));
        Assert.AreEqual(1, receivedMessages);
    }

    [TestMethod]
    public void SubscriptionMessageIsEmitted()
    {
        TwitchChatInterface chatInterface = CreateInterface();
        var receivedMessages = new List<ChatMessage>();
        chatInterface.MessageReceived += (_, message) => receivedMessages.Add(message);
        _client.OnNewSubscriber += Raise.With(
            _client,
            new OnNewSubscriberArgs
            {
                Channel = "atwitchchannel",
                Subscriber = new Subscriber(
                    new List<KeyValuePair<string, string>>(),
                    new List<KeyValuePair<string, string>>(),
                    "000000",
                    Color.Black,
                    "ATwitchUser",
                    "n/a",
                    "MSG-ID",
                    "atwitchuser",
                    string.Empty,
                    "sub", // see Twitch Chat API docs
                    "5",
                    "5",
                    true,
                    "SYS-MSG",
                    "Here is a resub message",
                    SubscriptionPlan.Tier2,
                    "Tier 2",
                    "ROOM-ID",
                    "USER-ID",
                    false,
                    false,
                    true,
                    false,
                    "111111111",
                    UserType.Viewer,
                    "RAW-IRC",
                    "atwitchchannel"
                ),
            }
        );

        Assert.AreEqual(1, receivedMessages.Count);
        Assert.IsTrue(receivedMessages.All(m => m.EventName == TwitchEventTypes.SUBSCRIPTION));

        var message = receivedMessages.First();
        Assert.AreEqual(
            new ChatMessage(
                "Twitch/atwitchchannel/MSG-ID",
                new ChatUser("Twitch/USER-ID", "ATwitchUser", PrivilegeLevel.Viewer),
                TwitchEventTypes.SUBSCRIPTION,
                "{\"SubscriptionPlanTier\":{\"Tier\":20,\"Name\":\"Tier 2\"},\"EventName\":\"Twitch.Subscription\"}",
                null,
                chatInterface.SupportedFeatures
            ),
            message
        );

        var parsedObject = JsonSerializer.Deserialize<TwitchSubscriptionReceivedEvent>(
            message.Content
        );

        Assert.IsNotNull(parsedObject);
        Assert.AreEqual(
            new TwitchSubscriptionReceivedEvent(
                new TwitchSubscriptionPlan(TwitchSubscriptionPlanTier.Tier2, "Tier 2")
            ),
            parsedObject
        );
    }

    private TwitchChatInterface CreateInterface(TwitchConfiguration? configuration = null)
    {
        if (configuration != null)
        {
            _configuration = configuration;
        }

        return new TwitchChatInterface(
            LoggingUtils.GetLogger<TwitchChatInterface>(),
            LoggingUtils.GetLogger<TwitchClient>(),
            _client,
            _configuration,
            _onlineMonitor,
            A.Fake<ITwitchApiService>()
        );
    }

    private OnMessageReceivedArgs CreateChatMessage(
        string authorUsername,
        string customMessage,
        UserType userType = UserType.Viewer,
        bool isSubscriber = false,
        bool isModerator = false,
        bool isMe = false,
        bool isBroadcaster = false,
        bool isVip = false,
        bool isPartner = false,
        bool isStaff = false
    )
    {
        return new OnMessageReceivedArgs
        {
            ChatMessage = new TwitchLib.Client.Models.ChatMessage(
                _configuration.Username,
                "userid",
                authorUsername,
                authorUsername,
                "#000000",
                Color.Black,
                new EmoteSet(Enumerable.Empty<Emote>(), "nothing"),
                customMessage,
                userType,
                "achannel",
                "anid",
                isSubscriber,
                0,
                "aRoomId",
                false,
                isModerator,
                isMe,
                isBroadcaster,
                isVip,
                isPartner,
                isStaff,
                Noisy.NotSet,
                "anIrcMessage",
                string.Empty,
                new List<KeyValuePair<string, string>>(),
                null,
                0,
                0
            ),
        };
    }

    private void CheckPrivilegeLevel(
        OnMessageReceivedArgs messageReceivedArgs,
        PrivilegeLevel expectedPrivilegeLevel
    )
    {
        TwitchChatInterface chatInterface = CreateInterface();

        var messages = new List<Entities.ChatMessage>();
        chatInterface.MessageReceived += (_, chatMessage) => messages.Add(chatMessage);

        _client.OnMessageReceived += Raise.With(messageReceivedArgs);

        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(
            new Entities.ChatMessage(
                messages[0].Identifier,
                new ChatUser(
                    new ChannelIdentifier(
                        TwitchChatInterface.IF_NAME,
                        messageReceivedArgs.ChatMessage.Username
                    ),
                    messageReceivedArgs.ChatMessage.Username,
                    expectedPrivilegeLevel
                ),
                SharedEventTypes.CHAT_MESSAGE,
                messageReceivedArgs.ChatMessage.Message
            ),
            messages.First()
        );
    }
}
