namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record MessageInterfaceDescription(
    string Name,
    string FullName,
    string Type,
    bool Enabled,
    string SupportedFeatures,
    int? CurrentBufferSize,
    int? MaxBufferSize,
    int? ThreadId);
