namespace FloppyBot.TwitchApi.Storage.Entities;

public enum TwitchCredentialValidation
{
    Unknown,
    Valid,
    Invalid,
    Expired,
    Missing,
}

public record TwitchCredentialValidationResult
{
    private TwitchCredentialValidationResult(
        TwitchCredentialValidation verdict,
        DateTimeOffset? expiresOn
    )
    {
        Verdict = verdict;
        ExpiresOn = expiresOn;
    }

    public static TwitchCredentialValidationResult Valid(DateTimeOffset expiresOn)
    {
        return new TwitchCredentialValidationResult(TwitchCredentialValidation.Valid, expiresOn);
    }

    public static TwitchCredentialValidationResult Invalid =>
        new(TwitchCredentialValidation.Invalid, null);

    public static TwitchCredentialValidationResult Expired(DateTimeOffset expiredOn) =>
        new(TwitchCredentialValidation.Expired, expiredOn);

    public static TwitchCredentialValidationResult Missing =>
        new(TwitchCredentialValidation.Missing, null);
    public static TwitchCredentialValidationResult Unknown =>
        new(TwitchCredentialValidation.Unknown, null);

    public TwitchCredentialValidation Verdict { get; init; }
    public DateTimeOffset? ExpiresOn { get; init; }

    public void Deconstruct(out TwitchCredentialValidation verdict, out DateTimeOffset? expiresOn)
    {
        verdict = Verdict;
        expiresOn = ExpiresOn;
    }
}
