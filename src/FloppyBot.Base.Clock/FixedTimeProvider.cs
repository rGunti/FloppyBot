namespace FloppyBot.Base.Clock;

/// <summary>
/// A time provider built for unit-testing. It provides a fixed, but adjustable time.
/// </summary>
/// <seealso cref="ITimeProvider"/>
public class FixedTimeProvider : ITimeProvider
{
    private DateTimeOffset _dateTime;

    public FixedTimeProvider(DateTimeOffset? dateTime = null)
    {
        _dateTime = dateTime ?? DateTimeOffset.MinValue;
    }

    /// <inheritdoc cref="ITimeProvider.GetCurrentTime"/>
    public DateTimeOffset GetCurrentTime()
    {
        return _dateTime;
    }

    /// <inheritdoc cref="ITimeProvider.GetCurrentUtcTime"/>
    public DateTimeOffset GetCurrentUtcTime()
    {
        return _dateTime.ToUniversalTime();
    }

    /// <summary>
    /// Changes the current time to the <see cref="DateTime"/> provided
    /// </summary>
    /// <param name="dateTime"></param>
    public void SetTime(DateTimeOffset dateTime)
    {
        _dateTime = dateTime;
    }

    /// <summary>
    /// Changes the current time by the given amount
    /// </summary>
    /// <param name="timeSpan"></param>
    public void AdvanceTimeBy(TimeSpan timeSpan)
    {
        _dateTime = _dateTime.Add(timeSpan);
    }

    public override string ToString()
    {
        return $"{nameof(FixedTimeProvider)}: {_dateTime.ToUniversalTime()}";
    }
}
