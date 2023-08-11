using FakeItEasy;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Timer.Storage;
using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Aux.Timer.Tests;

[TestClass]
public class TimerCommandsTests
{
    private readonly TimerCommands _host;
    private readonly ITimerService _timerService;

    public TimerCommandsTests()
    {
        _timerService = A.Fake<ITimerService>();
        _host = new TimerCommands(LoggingUtils.GetLogger<TimerCommands>(), _timerService);
    }

    [TestMethod]
    public void CreateNewTimer()
    {
        CommandResult result = _host.CreateTimer(
            "12m",
            "Hello World",
            new ChatUser("Mock/User", "User", PrivilegeLevel.Moderator),
            "Mock/Channel/Message"
        );

        Assert.AreEqual(
            CommandResult.SuccessWith(
                "Created new timer. Your message should be there in about 12 minutes."
            ),
            result
        );
        A.CallTo(
                () =>
                    _timerService.CreateTimer(
                        "Mock/Channel/Message",
                        "Mock/User",
                        TimeSpan.FromMinutes(12),
                        "Hello World"
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [DataTestMethod]
    [DataRow("25", DisplayName = "No units")]
    [DataRow("25m12d4h", DisplayName = "Wrong order of units")]
    public void RepliesWithErrorWhenParsingFails(string input)
    {
        Assert.AreEqual(
            CommandResult.FailedWith(TimerCommands.REPLY_FAILED_TIMESPAN),
            _host.CreateTimer(
                input,
                "Some Text",
                new ChatUser("Mock/User", "User", PrivilegeLevel.Moderator),
                "Mock/Channel/Message"
            )
        );
    }
}
