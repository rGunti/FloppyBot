namespace FloppyBot.WebApi.Base.Dtos;

public record UnavailableFeatureResponse(
        int StatusCode,
        string Message,
        string? Source = null)
    : ErrorResponse(StatusCode, Message, Source);
