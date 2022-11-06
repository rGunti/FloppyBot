namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record FileHeader(
    string Id,
    string ChannelId,
    string FileName,
    long FileSize,
    string MimeType);
