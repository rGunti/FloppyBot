using System.Globalization;
using FakeItEasy;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Extensions;
using FloppyBot.Base.Testing;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Time.Storage;
using FloppyBot.Commands.Aux.Time.Storage.Entities;
using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Aux.Time.Tests;

[TestClass]
public class TimeCommandsTest
{
    private static readonly DateTimeOffset ReferenceTime = DateTimeOffset.Parse(
        "2022-12-05T12:34:56Z",
        CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal
    );

    private readonly TimeCommands _host;
    private readonly IUserTimeZoneSettingsService _service;
    private readonly FixedTimeProvider _timeProvider;

    public TimeCommandsTest()
    {
        _timeProvider = new FixedTimeProvider(ReferenceTime);
        _service = A.Fake<IUserTimeZoneSettingsService>();
        _host = new TimeCommands(LoggingUtils.GetLogger<TimeCommands>(), _timeProvider, _service);

        A.CallTo(() => _service.GetTimeZoneForUser(A<string>.Ignored))
            .Returns(NullableObject.Empty<UserTimeZoneSetting>());
    }

    [DataTestMethod]
    [DataRow(null, "The current time is 12:34 in Coordinated Universal Time")]
    [DataRow("Europe/Berlin", "The current time is 13:34 in Central European Standard Time")]
    [DataRow("America/New_York", "The current time is 07:34 in Eastern Standard Time")]
    public void OutputsCurrentTime(string? timeZoneId, string expectedOutput)
    {
        Assert.AreEqual(
            CommandResult.SuccessWith(expectedOutput),
            _host.ShowCurrentTime(
                new ChatUser("Mock/User", "User", PrivilegeLevel.Viewer),
                timeZoneId
            )
        );
    }

    [TestMethod]
    public void OutputsUserTimeZone()
    {
        A.CallTo(() => _service.GetTimeZoneForUser(A<string>.Ignored))
            .ReturnsLazily((string userId) => new UserTimeZoneSetting(userId, "Asia/Tokyo"));

        Assert.AreEqual(
            CommandResult.SuccessWith("The current time is 21:34 in Japan Standard Time"),
            _host.ShowCurrentTime(new ChatUser("Mock/User", "User", PrivilegeLevel.Viewer), null)
        );
    }

    [DataTestMethod]
    [DataRow(null, "The current time is 05:24 DEC in Coordinated Universal Time")]
    [DataRow("Europe/Berlin", "The current time is 05:65 DEC in Central European Standard Time")]
    [DataRow("America/New_York", "The current time is 03:15 DEC in Eastern Standard Time")]
    public void OutputsCurrentDecimalTime(string? timeZoneId, string expectedOutput)
    {
        Assert.AreEqual(
            CommandResult.SuccessWith(expectedOutput),
            _host.ShowCurrentDecimalTime(
                new ChatUser("Mock/User", "User", PrivilegeLevel.Viewer),
                timeZoneId
            )
        );
    }

    [TestMethod]
    public void SetUserTimeZone()
    {
        CommandResult result = _host.SetUserTimeZone(
            new ChatUser("Mock/User", "User", PrivilegeLevel.Viewer),
            "America/Lima"
        );

        Assert.AreEqual(
            CommandResult.SuccessWith("Your timezone is now set to America/Lima"),
            result
        );

        A.CallTo(() => _service.SetTimeZoneForUser("Mock/User", "America/Lima"))
            .MustHaveHappenedOnceExactly();
    }
}
