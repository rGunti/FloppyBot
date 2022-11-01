namespace FloppyBot.Base.Clock;

/// <summary>
/// A time provider providing the current system time
/// </summary>
/// <seealso cref="ITimeProvider"/>
public class RealTimeProvider : ITimeProvider
{
    /// <inheritdoc cref="ITimeProvider.GetCurrentTime"/>
    public DateTimeOffset GetCurrentTime()
    {
        return DateTimeOffset.Now;
    }

    /// <inheritdoc cref="ITimeProvider.GetCurrentUtcTime"/>
    public DateTimeOffset GetCurrentUtcTime()
    {
        return DateTimeOffset.UtcNow;
    }
}
