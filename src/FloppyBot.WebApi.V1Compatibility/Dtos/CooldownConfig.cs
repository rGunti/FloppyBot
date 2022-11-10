namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record CooldownConfig(
    int Cooldown,
    int ModCooldown,
    int AdminCooldown);
