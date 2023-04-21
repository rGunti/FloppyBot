namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CooldownInfo(
    string CooldownMode,
    int? DefaultCooldown,
    int? ModCooldown,
    int? AdminCooldown
);
