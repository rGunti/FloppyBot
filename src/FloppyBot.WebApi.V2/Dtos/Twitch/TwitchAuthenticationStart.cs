namespace FloppyBot.WebApi.V2.Dtos.Twitch;

public record TwitchAuthenticationStart(string LoginUrl);

public record TwitchAuthenticationConfirm(string SessionId, string Code);
