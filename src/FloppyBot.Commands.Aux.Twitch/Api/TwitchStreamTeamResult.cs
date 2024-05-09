namespace FloppyBot.Commands.Aux.Twitch.Api;

public record TwitchStreamTeamResult(
    string AccountName,
    string? TeamId,
    string? TeamSlug,
    string? TeamName
)
{
    public string? Link =>
        !string.IsNullOrWhiteSpace(TeamSlug) ? $"https://www.twitch.tv/team/{TeamSlug}" : null;

    public bool IsSameTeam(TwitchStreamTeamResult team)
    {
        return TeamId == team.TeamId;
    }
}
