using FloppyBot.Commands.Aux.Twitch.Api;

namespace FloppyBot.Commands.Aux.Twitch.Helpers;

public record MessageTemplateParams(
    string AccountName,
    string DisplayName,
    string? LastGame,
    string? TeamId,
    string? TeamSlug,
    string? TeamName
)
{
    public string Link => $"https://twitch.tv/{AccountName}";
    public string? TeamLink =>
        !string.IsNullOrWhiteSpace(TeamSlug) ? $"https://www.twitch.tv/team/{TeamSlug}" : null;

    public static MessageTemplateParams FromLookups(
        TwitchUserLookupResult userLookup,
        TwitchStreamTeamResult? teamLookup
    )
    {
        return new MessageTemplateParams(
            userLookup.AccountName,
            userLookup.DisplayName,
            userLookup.LastGame,
            teamLookup?.TeamId,
            teamLookup?.TeamSlug,
            teamLookup?.TeamName
        );
    }
}
