namespace FloppyBot.Base.Clock;

/// <summary>
/// An interface that allows the acquisition of time
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// Gets the current time
    /// </summary>
    /// <returns></returns>
    DateTimeOffset GetCurrentTime();

    /// <summary>
    /// Gets the current time in UTC
    /// </summary>
    /// <returns></returns>
    DateTimeOffset GetCurrentUtcTime();
}
