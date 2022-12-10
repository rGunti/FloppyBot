using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Timer.Storage;
using FloppyBot.Commands.Core.Entities;
using Moq;

namespace FloppyBot.Commands.Aux.Timer.Tests;

[TestClass]
public class TimerCommandsTests
{
    private readonly TimerCommands _host;
    private readonly Mock<ITimerService> _timerServiceMock;

    public TimerCommandsTests()
    {
        _timerServiceMock = new Mock<ITimerService>();
        _host = new TimerCommands(
            LoggingUtils.GetLogger<TimerCommands>(),
            _timerServiceMock.Object);
    }

    [TestMethod]
    public void CreateNewTimer()
    {
        _timerServiceMock
            .Setup(s => s.CreateTimer(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<string>()))
            .Verifiable();

        CommandResult result = _host.CreateTimer(
            "12m",
            "Hello World",
            new ChatUser(
                "Mock/User",
                "User",
                PrivilegeLevel.Moderator),
            "Mock/Channel/Message");

        Assert.AreEqual(
            CommandResult.SuccessWith("Created new timer. Your message should be there in about 12 minutes."),
            result);
        _timerServiceMock
            .Verify(s => s.CreateTimer(
                    It.Is<string>(i => i == "Mock/Channel/Message"),
                    It.Is<string>(i => i == "Mock/User"),
                    It.Is<TimeSpan>(t => t == TimeSpan.FromMinutes(12)),
                    It.Is<string>(i => i == "Hello World")),
                Times.Once);
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
                "Mock/Channel/Message"));
    }
}

