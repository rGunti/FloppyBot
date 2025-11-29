using System;
using FloppyBot.Chat.Entities.Identifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FloppyBot.Chat.Tests.Entities.Identifiers;

[TestClass]
public class IdentifierTests
{
    [TestMethod]
    [DataRow("Twitch/pinsrltrex", "Twitch", "pinsrltrex")]
    public void ConvertStringToChannelId(
        string inputChannelId,
        string expectedInterface,
        string expectedChannel
    )
    {
        ChannelIdentifier channelId = inputChannelId;
        var expectedChannelId = new ChannelIdentifier(expectedInterface, expectedChannel);

        Assert.AreEqual(expectedChannelId, channelId);
        Assert.AreEqual(inputChannelId, (string)channelId);
    }

    [TestMethod]
    [DataRow("Twitch")]
    [DataRow("Twitch/pinsrltrex/1234")]
    public void ConvertStringToChannelIdThrowsException(string inputChannelId)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ChannelIdentifier _ = inputChannelId;
        });
    }

    [TestMethod]
    [DataRow("Twitch/pinsrltrex/1234", "Twitch", "pinsrltrex", "1234")]
    public void ConvertStringToExtendedChannelId(
        string inputChannelId,
        string expectedInterface,
        string expectedChannel,
        params string[] expectedAdditionalInfo
    )
    {
        ExtendedChannelIdentifier channelId = inputChannelId;
        var expectedChannelId = new ExtendedChannelIdentifier(
            expectedInterface,
            expectedChannel,
            expectedAdditionalInfo
        );
        Assert.AreEqual(expectedChannelId, channelId);
        Assert.AreEqual(inputChannelId, (string)channelId);
    }

    [TestMethod]
    [DataRow("Twitch")]
    public void ConvertStringToExtendedChannelIdThrowsException(string inputChannelId)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ExtendedChannelIdentifier _ = inputChannelId;
        });
    }

    [TestMethod]
    [DataRow("Twitch/pinsrltrex/1234", "Twitch", "pinsrltrex", "1234")]
    public void ConvertStringToMessageId(
        string inputMessageId,
        string expectedInterface,
        string expectedChannel,
        string expectedMessage
    )
    {
        ChatMessageIdentifier messageId = inputMessageId;
        var expectedMessageId = new ChatMessageIdentifier(
            expectedInterface,
            expectedChannel,
            expectedMessage
        );

        Assert.AreEqual(expectedMessageId, messageId);
        Assert.AreEqual(inputMessageId, (string)messageId);
    }

    [TestMethod]
    [DataRow("Twitch")]
    [DataRow("Twitch/pinsrltrex")]
    public void ConvertStringToMessageIdThrowsException(string inputMessageId)
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ChatMessageIdentifier _ = inputMessageId;
        });
    }

    [TestMethod]
    [DataRow("Twitch/pinsrltrex/1234", "Twitch/pinsrltrex")]
    public void GetChannelOfMessageIdentifier(string inputMessageId, string expectedChannelId)
    {
        ChatMessageIdentifier messageId = inputMessageId;
        Assert.AreEqual((ChannelIdentifier)expectedChannelId, messageId.GetChannel());
    }
}
