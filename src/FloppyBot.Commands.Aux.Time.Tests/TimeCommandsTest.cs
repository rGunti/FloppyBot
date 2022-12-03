using System.Globalization;
using FloppyBot.Base.Clock;
using FloppyBot.Base.Testing;

namespace FloppyBot.Commands.Aux.Time.Tests;

[TestClass]
public class TimeCommandsTest
{
    private static readonly DateTimeOffset ReferenceTime = DateTimeOffset.Parse(
        "2022-12-05T12:34:56Z",
        CultureInfo.InvariantCulture,
        DateTimeStyles.AssumeUniversal);

    private readonly TimeCommands _host;
    private readonly FixedTimeProvider _timeProvider;

    public TimeCommandsTest()
    {
        _timeProvider = new FixedTimeProvider(ReferenceTime);
        _host = new TimeCommands(
            LoggingUtils.GetLogger<TimeCommands>(),
            _timeProvider);
    }

    [DataTestMethod]
    [DataRow(null, "The current time is 12:34 in Coordinated Universal Time")]
    [DataRow("Europe/Berlin", "The current time is 13:34 in Central European Standard Time")]
    [DataRow("America/New_York", "The current time is 07:34 in Eastern Standard Time")]
    public void OutputsCurrentTime(
        string? timeZoneId,
        string expectedOutput)
    {
        Assert.AreEqual(
            expectedOutput,
            _host.ShowCurrentTime(timeZoneId).ResponseContent);
    }
}

