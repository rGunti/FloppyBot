namespace FloppyBot.TwitchApi.Exceptions;

public class TwitchAuthenticationException : Exception
{
    private TwitchAuthenticationException(string message, Exception? innerException = null)
        : base(message, innerException) { }

    public static TwitchAuthenticationException NoSession()
    {
        return new TwitchAuthenticationException(
            "No valid init session found for given user and channel"
        );
    }

    public static TwitchAuthenticationException FailedToAcquireAccessToken(
        Exception? innerException = null
    )
    {
        return new TwitchAuthenticationException("Failed to acquire access token", innerException);
    }

    public static TwitchAuthenticationException NoCredentialsForChannel(string channel)
    {
        return new TwitchAuthenticationException(
            $"No access credentials found for channel {channel}"
        );
    }

    public static TwitchAuthenticationException FailedToRefreshAccessToken(
        Exception? innerException = null
    )
    {
        return new TwitchAuthenticationException("Failed to refresh access token", innerException);
    }
}
