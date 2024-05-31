using FakeItEasy;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Abstraction.Entities;
using FloppyBot.Chat.Entities;
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
    private readonly IAuditor _auditor;

    public ShoutoutCommandTests()
    {
        _auditor = A.Fake<IAuditor>();

        var twitchApiService = A.Fake<ITwitchApiService>();
        _shoutoutMessageSettingService = A.Fake<IShoutoutMessageSettingService>();
        _shoutoutCommand = new ShoutoutCommand(
            twitchApiService,
            _shoutoutMessageSettingService,
            A.Fake<ILogger<ShoutoutCommand>>(),
            _auditor
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
        A.CallTo(
                () =>
                    twitchApiService.LookupTeam(
                        A<string>.That.Matches(s => s == "somestreamer" || s == "someteamuser")
                    )
            )
            .ReturnsLazily(
                (string accountName) =>
                    Task.FromResult<TwitchStreamTeamResult?>(
                        new TwitchStreamTeamResult(
                            accountName,
                            "1234567890",
                            "coolteam",
                            "The Really Cool Stream Team"
                        )
                    )
            );
        A.CallTo(() => _shoutoutMessageSettingService.GetSettings("Twitch/someuser"))
            .ReturnsLazily(
                () =>
                    new ShoutoutMessageSetting(
                        "Twitch/someuser",
                        "Check out {DisplayName} at {Link}! They last played {LastGame}!",
                        null
                    )
            );
        A.CallTo(
                () =>
                    _shoutoutMessageSettingService.GetSettings(
                        A<string>.That.Matches(s =>
                            s == "Twitch/someteamuser" || s == "Twitch/somestreamer"
                        )
                    )
            )
            .ReturnsLazily(
                () =>
                    new ShoutoutMessageSetting(
                        "Twitch/someteamuser",
                        "Check out {DisplayName} at {Link}! They last played {LastGame}!",
                        "Check out my team mate {DisplayName} at {Link}! They last played {LastGame}! Also go checkout {TeamName} at {TeamLink}!"
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
    public async Task RepliesWithTeamShoutoutMessage()
    {
        var reply = await _shoutoutCommand.Shoutout("Twitch/someteamuser", "somestreamer");
        Assert.AreEqual(
            "Check out my team mate SomeStreamer at https://twitch.tv/somestreamer! They last played Cool Game! Also go checkout The Really Cool Stream Team at https://www.twitch.tv/team/coolteam!",
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
        var reply = _shoutoutCommand.SetShoutout(
            new ChatUser("Twitch/someuser", "Some User", PrivilegeLevel.Moderator),
            "Twitch/someuser",
            "My new template"
        );

        Assert.AreEqual(ShoutoutCommand.REPLY_SAVE, reply);
        A.CallTo(
                () =>
                    _shoutoutMessageSettingService.SetShoutoutMessage(
                        "Twitch/someuser",
                        "My new template"
                    )
            )
            .MustHaveHappenedOnceExactly();
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Twitch/someuser",
                            "Twitch/someuser",
                            TwitchAuditing.ShoutoutMessageType,
                            "Message",
                            CommonActions.Updated,
                            "My new template"
                        )
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void ConfigureTeamShoutoutSavesToDatabase()
    {
        var reply = _shoutoutCommand.SetTeamShoutout(
            new ChatUser("Twitch/someuser", "Some User", PrivilegeLevel.Moderator),
            "Twitch/someuser",
            "My new team template"
        );

        Assert.AreEqual(ShoutoutCommand.REPLY_SAVE, reply);
        A.CallTo(
                () =>
                    _shoutoutMessageSettingService.SetShoutoutMessage(
                        A<ShoutoutMessageSetting>.That.Matches(setting =>
                            setting.Id == "Twitch/someuser"
                            && setting.TeamMessage == "My new team template"
                        )
                    )
            )
            .MustHaveHappenedOnceExactly();
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Twitch/someuser",
                            "Twitch/someuser",
                            TwitchAuditing.ShoutoutMessageType,
                            "TeamMessage",
                            CommonActions.Updated,
                            "My new team template"
                        )
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void ClearIssuesDeleteCommand()
    {
        var reply = _shoutoutCommand.ClearShoutout(
            new ChatUser("Twitch/someuser", "Some User", PrivilegeLevel.Moderator),
            "Twitch/someuser"
        );

        Assert.AreEqual(ShoutoutCommand.REPLY_CLEAR, reply);
        A.CallTo(() => _shoutoutMessageSettingService.ClearSettings("Twitch/someuser"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Twitch/someuser",
                            "Twitch/someuser",
                            TwitchAuditing.ShoutoutMessageType,
                            string.Empty,
                            CommonActions.Deleted,
                            null
                        )
                    )
            )
            .MustHaveHappenedOnceExactly();
    }
}
