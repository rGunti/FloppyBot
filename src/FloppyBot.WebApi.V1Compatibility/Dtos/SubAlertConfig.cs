namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record SubAlertConfig(
    string Id,
    string Message,
    string ReSubMessage,
    string CommunitySubMessage,
    string GiftSubMessage
);
