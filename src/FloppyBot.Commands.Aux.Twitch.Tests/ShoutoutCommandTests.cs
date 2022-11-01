using FloppyBot.Commands.Aux.Twitch.Api;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using Moq;

namespace FloppyBot.Commands.Aux.Twitch.Tests;

[TestClass]
public class ShoutoutCommandTests
{
    private readonly ShoutoutCommand _shoutoutCommand;
    private readonly Mock<IShoutoutMessageSettingService> _shoutoutMessageSettingServiceMock;
    private readonly Mock<ITwitchApiService> _twitchApiServiceMock;

    public ShoutoutCommandTests()
    {
        _twitchApiServiceMock = new Mock<ITwitchApiService>();
        _shoutoutMessageSettingServiceMock = new Mock<IShoutoutMessageSettingService>();
        _shoutoutCommand = new ShoutoutCommand(
            _twitchApiServiceMock.Object,
            _shoutoutMessageSettingServiceMock.Object);

        _twitchApiServiceMock
            .Setup(s => s.LookupUser(It.Is<string>(c => c == "somestreamer")))
            .Returns((string _) => Task.FromResult(new TwitchUserLookupResult(
                "somestreamer",
                "SomeStreamer",
                "Cool Game"))!);
        _shoutoutMessageSettingServiceMock
            .Setup(s => s.GetSettings(It.Is<string>(c => c == "Twitch/someuser")))
            .Returns((string _) => new ShoutoutMessageSetting(
                "Twitch/someuser",
                "Check out {DisplayName} at {Link}! They last played {LastGame}!"));
    }

    [TestMethod]
    public async Task RepliesWithShoutoutMessage()
    {
        var reply = await _shoutoutCommand.Shoutout("Twitch/someuser", "somestreamer");
        Assert.AreEqual(
            "Check out SomeStreamer at https://twitch.tv/somestreamer! They last played Cool Game!",
            reply);
    }

    [TestMethod]
    public async Task DoNotReplyIfNoMessageIsConfigured()
    {
        Assert.IsNull(await _shoutoutCommand.Shoutout("Twitch/somestreamer", "someuser"));
    }

    [TestMethod]
    public async Task DoNotReplyIfNoUserCanBeFound()
    {
        Assert.IsNull(await _shoutoutCommand.Shoutout("Twitch/someuser", "someuser"));
    }

    [TestMethod]
    public void ConfigureCreatesDatabaseRecord()
    {
        _shoutoutMessageSettingServiceMock
            .Setup(s => s.SetShoutoutMessage(It.IsAny<string>(), It.IsAny<string>()))
            .Verifiable();
        var reply = _shoutoutCommand.SetShoutout(
            "Twitch/someuser",
            "My new template");

        Assert.AreEqual(ShoutoutCommand.REPLY_SAVE, reply);
        _shoutoutMessageSettingServiceMock
            .Verify(
                s => s.SetShoutoutMessage(
                    It.Is<string>(c => c == "Twitch/someuser"),
                    It.Is<string>(m => m == "My new template")),
                Times.Once);
    }

    [TestMethod]
    public void ClearIssuesDeleteCommand()
    {
        _shoutoutMessageSettingServiceMock
            .Setup(s => s.ClearSettings(It.IsAny<string>()))
            .Verifiable();
        var reply = _shoutoutCommand.ClearShoutout("Twitch/someuser");

        Assert.AreEqual(ShoutoutCommand.REPLY_CLEAR, reply);
        _shoutoutMessageSettingServiceMock
            .Verify(
                s => s.ClearSettings(It.Is<string>(c => c == "Twitch/someuser")),
                Times.Once);
    }
}
