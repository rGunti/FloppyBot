namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record QuoteDto(
    string Id,
    string Channel,
    int QuoteId,
    string QuoteText,
    string? QuoteContext,
    DateTimeOffset CreatedAt,
    string CreatedBy);
