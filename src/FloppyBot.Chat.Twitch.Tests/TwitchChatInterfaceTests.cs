using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Chat.Entities.Identifiers;
using FloppyBot.Chat.Twitch.Config;
using FloppyBot.Chat.Twitch.Monitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TwitchLib.Client;
using TwitchLib.Client.Enums;
using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using ChatMessage = TwitchLib.Client.Models.ChatMessage;

namespace FloppyBot.Chat.Twitch.Tests;

[TestClass]
public class TwitchChatInterfaceTests
{
    private readonly Mock<ITwitchClient> _clientMock;
    private readonly Mock<ITwitchChannelOnlineMonitor> _onlineMonitorMock;
    private TwitchConfiguration _configuration;

    public TwitchChatInterfaceTests()
    {
        _clientMock = new Mock<ITwitchClient>();
        _onlineMonitorMock = new Mock<ITwitchChannelOnlineMonitor>();
        _configuration =
            new TwitchConfiguration(
                "atwitchbot",
                "sometoken",
                "atwitchchannel",
                "aclientid",
                "anaccesstoken",
                false,
                0);
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
            _clientMock.Object,
            _configuration,
            _onlineMonitorMock.Object);
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
        bool isStaff = false)
    {
        return new OnMessageReceivedArgs
        {
            ChatMessage = new ChatMessage(
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
                0)
        };
    }

    private void CheckPrivilegeLevel(
        OnMessageReceivedArgs messageReceivedArgs,
        PrivilegeLevel expectedPrivilegeLevel)
    {
        TwitchChatInterface chatInterface = CreateInterface();

        var messages = new List<Entities.ChatMessage>();
        chatInterface.MessageReceived += (_, chatMessage) => messages.Add(chatMessage);

        _clientMock
            .Raise(
                c => c.OnMessageReceived += null,
                messageReceivedArgs);

        Assert.AreEqual(1, messages.Count);
        Assert.AreEqual(
            new Entities.ChatMessage(
                messages[0].Identifier,
                new ChatUser(
                    new ChannelIdentifier(
                        TwitchChatInterface.IF_NAME,
                        messageReceivedArgs.ChatMessage.Username),
                    messageReceivedArgs.ChatMessage.Username,
                    expectedPrivilegeLevel),
                TwitchChatInterface.EVENT_MESSAGE,
                messageReceivedArgs.ChatMessage.Message),
            messages.First());
    }

    [TestMethod]
    public void BroadcasterHasAdminRights()
    {
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world",
                isBroadcaster: true),
            PrivilegeLevel.Administrator);
    }

    [TestMethod]
    public void ModeratorHasModRights()
    {
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world",
                isModerator: true),
            PrivilegeLevel.Moderator);
    }

    [TestMethod]
    public void ViewerHasViewerRights()
    {
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world"),
            PrivilegeLevel.Viewer);
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world",
                isPartner: true),
            PrivilegeLevel.Viewer);
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world",
                isStaff: true),
            PrivilegeLevel.Viewer);
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world",
                isSubscriber: true),
            PrivilegeLevel.Viewer);
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world",
                isPartner: true,
                isStaff: true,
                isSubscriber: true),
            PrivilegeLevel.Viewer);
    }

    [TestMethod]
    public void BotUserHasNoRights()
    {
        CheckPrivilegeLevel(
            CreateChatMessage("atwitchviewer", "hello world",
                isMe: true),
            PrivilegeLevel.Unknown);
    }

    [TestMethod]
    public void MessageIsSent()
    {
        TwitchChatInterface chatInterface = CreateInterface();

        _clientMock
            .Setup(c => c.SendMessage(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Verifiable();

        chatInterface.SendMessage("A message\n\nwith multiple lines");

        _clientMock
            .Verify(c => c.SendMessage(
                It.Is<string>(s => s == _configuration.Channel),
                It.Is<string>(s => s == "A message"),
                It.IsAny<bool>()),
                Times.Once);
        _clientMock
            .Verify(c => c.SendMessage(
                It.Is<string>(s => s == _configuration.Channel),
                It.Is<string>(s => s == "with multiple lines"),
                It.IsAny<bool>()),
                Times.Once);
    }

    [TestMethod]
    public void MessageIsNotEmittedIfOffline()
    {
        TwitchChatInterface chatInterface = CreateInterface(
            _configuration with
            {
                DisableWhenChannelIsOffline = true
            });
        var receivedMessages = 0;
        chatInterface.MessageReceived += (_, _) => receivedMessages++;

        _onlineMonitorMock
            .Setup(m => m.IsChannelOnline())
            .Returns(false);

        _clientMock
            .Raise(
                c => c.OnMessageReceived += null,
                CreateChatMessage("atwitchviewer", "hello world"));
        Assert.AreEqual(0, receivedMessages);

        _onlineMonitorMock
            .Setup(m => m.IsChannelOnline())
            .Returns(true);

        _clientMock
            .Raise(
                c => c.OnMessageReceived += null,
                CreateChatMessage("atwitchviewer", "hello world"));
        Assert.AreEqual(1, receivedMessages);
    }
}
