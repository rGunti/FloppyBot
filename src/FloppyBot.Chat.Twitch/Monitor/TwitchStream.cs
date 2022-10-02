using Stream = TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream;

namespace FloppyBot.Chat.Twitch.Monitor;

public record TwitchStream(
    bool IsOnline,
    string ChannelName,
    string UserLogin,
    string GameId,
    string GameName,
    string Title,
    int ViewerCount,
    DateTimeOffset StartedAt,
    string Language,
    bool IsMature);

public static class StreamExtensions
{
    public static TwitchStream ToTwitchStream(this Stream stream, bool isOnline)
    {
        return new TwitchStream(
            isOnline,
            stream.UserName,
            stream.UserLogin,
            stream.GameId,
            stream.GameName,
            stream.Title,
            stream.ViewerCount,
            stream.StartedAt,
            stream.Language,
            stream.IsMature);
    }
}
