namespace FloppyBot.Commands.Aux.Translation.Commands;

public record TranslationRequest(
    string? InputLanguage,
    string OutputLanguage,
    string Text)
{
    public override string ToString()
    {
        return $"Request {InputLanguage}->{OutputLanguage}: {Text}";
    }
}

public record TranslationResponse(
    TranslationRequest Request,
    string? Reply,
    string? DetectedInputLanguage);

