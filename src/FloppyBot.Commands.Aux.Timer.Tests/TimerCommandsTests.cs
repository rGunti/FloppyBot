using FakeItEasy;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Auditing.Abstraction.Entities;
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
    private readonly IAuditor _auditor;

    public TimerCommandsTests()
    {
        _timerService = A.Fake<ITimerService>();
        _auditor = A.Fake<IAuditor>();
        _host = new TimerCommands(LoggingUtils.GetLogger<TimerCommands>(), _timerService, _auditor);
    }

    [TestMethod]
    public void CreateNewTimer()
    {
        CommandResult result = _host.CreateTimer(
            "12m",
            "Hello World",
            new ChatUser("Mock/User", "User", PrivilegeLevel.Moderator),
            "Mock/Channel/Message",
            "Mock/Channel"
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
        A.CallTo(
                () =>
                    _auditor.Record(
                        new AuditRecord(
                            null!,
                            DateTimeOffset.MinValue,
                            "Mock/User",
                            "Mock/Channel",
                            TimerAuditing.TimerType,
                            "Mock/Channel/Message",
                            CommonActions.Created,
                            "[00:12:00]: Hello World"
                        )
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
                "Mock/Channel/Message",
                "Mock/Channel"
            )
        );

        A.CallTo(() => _auditor.Record(A<AuditRecord>._)).MustNotHaveHappened();
    }
}
