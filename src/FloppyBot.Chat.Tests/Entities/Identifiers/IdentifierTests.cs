using System;
using FloppyBot.Chat.Entities.Identifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FloppyBot.Chat.Tests.Entities.Identifiers;

[TestClass]
public class IdentifierTests
{
    [DataTestMethod]
    [DataRow("Twitch/pinsrltrex", "Twitch", "pinsrltrex")]
    public void ConvertStringToChannelId(
        string inputChannelId,
        string expectedInterface,
        string expectedChannel)
    {
        ChannelIdentifier channelId = inputChannelId;
        var expectedChannelId = new ChannelIdentifier(
            expectedInterface,
            expectedChannel);

        Assert.AreEqual(expectedChannelId, channelId);
        Assert.AreEqual(inputChannelId, (string)channelId);
    }

    [DataTestMethod]
    [DataRow("Twitch")]
    [DataRow("Twitch/pinsrltrex/1234")]
    public void ConvertStringToChannelIdThrowsException(string inputChannelId)
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            ChannelIdentifier _ = inputChannelId;
        });
    }

    [DataTestMethod]
    [DataRow("Twitch/pinsrltrex/1234", "Twitch", "pinsrltrex", "1234")]
    public void ConvertStringToExtendedChannelId(
        string inputChannelId,
        string expectedInterface,
        string expectedChannel,
        params string[] expectedAdditionalInfo)
    {
        ExtendedChannelIdentifier channelId = inputChannelId;
        var expectedChannelId = new ExtendedChannelIdentifier(
            expectedInterface,
            expectedChannel,
            expectedAdditionalInfo);
        Assert.AreEqual(expectedChannelId, channelId);
        Assert.AreEqual(inputChannelId, (string)channelId);
    }

    [DataTestMethod]
    [DataRow("Twitch")]
    public void ConvertStringToExtendedChannelIdThrowsException(string inputChannelId)
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            ExtendedChannelIdentifier _ = inputChannelId;
        });
    }

    [DataTestMethod]
    [DataRow("Twitch/pinsrltrex/1234", "Twitch", "pinsrltrex", "1234")]
    public void ConvertStringToMessageId(
        string inputMessageId,
        string expectedInterface,
        string expectedChannel,
        string expectedMessage)
    {
        ChatMessageIdentifier messageId = inputMessageId;
        var expectedMessageId = new ChatMessageIdentifier(
            expectedInterface,
            expectedChannel,
            expectedMessage);

        Assert.AreEqual(expectedMessageId, messageId);
        Assert.AreEqual(inputMessageId, (string)messageId);
    }

    [DataTestMethod]
    [DataRow("Twitch")]
    [DataRow("Twitch/pinsrltrex")]
    public void ConvertStringToMessageIdThrowsException(string inputMessageId)
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            ChatMessageIdentifier _ = inputMessageId;
        });
    }

    [DataTestMethod]
    [DataRow("Twitch/pinsrltrex/1234", "Twitch/pinsrltrex")]
    public void GetChannelOfMessageIdentifier(
        string inputMessageId,
        string expectedChannelId)
    {
        ChatMessageIdentifier messageId = inputMessageId;
        Assert.AreEqual((ChannelIdentifier)expectedChannelId, messageId.GetChannel());
    }
}
