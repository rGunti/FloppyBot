using FakeItEasy;
using FloppyBot.Commands.Aux.Twitch.Api;
using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.Commands.Aux.Twitch.Storage.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.Commands.Aux.Twitch.Tests;

[TestClass]
public class ShoutoutCommandTests
{
    private readonly ShoutoutCommand _shoutoutCommand;
    private readonly IShoutoutMessageSettingService _shoutoutMessageSettingService;

    public ShoutoutCommandTests()
    {
        var twitchApiService = A.Fake<ITwitchApiService>();
        _shoutoutMessageSettingService = A.Fake<IShoutoutMessageSettingService>();
        _shoutoutCommand = new ShoutoutCommand(
            twitchApiService,
            _shoutoutMessageSettingService,
            A.Fake<ILogger<ShoutoutCommand>>()
        );

        A.CallTo(() => twitchApiService.LookupUser(A<string>.Ignored))
            .ReturnsLazily((string _) => Task.FromResult<TwitchUserLookupResult?>(null));
        A.CallTo(() => twitchApiService.LookupUser("somestreamer"))
            .ReturnsLazily(
                (string _) =>
                    Task.FromResult<TwitchUserLookupResult?>(
                        new TwitchUserLookupResult("somestreamer", "SomeStreamer", "Cool Game")
                    )
            );
        A.CallTo(() => _shoutoutMessageSettingService.GetSettings("Twitch/someuser"))
            .ReturnsLazily(
                () =>
                    new ShoutoutMessageSetting(
                        "Twitch/someuser",
                        "Check out {DisplayName} at {Link}! They last played {LastGame}!"
                    )
            );
    }

    [TestMethod]
    public async Task RepliesWithShoutoutMessage()
    {
        var reply = await _shoutoutCommand.Shoutout("Twitch/someuser", "somestreamer");
        Assert.AreEqual(
            "Check out SomeStreamer at https://twitch.tv/somestreamer! They last played Cool Game!",
            reply
        );
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
        var reply = _shoutoutCommand.SetShoutout("Twitch/someuser", "My new template");

        Assert.AreEqual(ShoutoutCommand.REPLY_SAVE, reply);
        A.CallTo(
                () =>
                    _shoutoutMessageSettingService.SetShoutoutMessage(
                        "Twitch/someuser",
                        "My new template"
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void ClearIssuesDeleteCommand()
    {
        var reply = _shoutoutCommand.ClearShoutout("Twitch/someuser");

        Assert.AreEqual(ShoutoutCommand.REPLY_CLEAR, reply);
        A.CallTo(() => _shoutoutMessageSettingService.ClearSettings("Twitch/someuser"))
            .MustHaveHappenedOnceExactly();
    }
}
