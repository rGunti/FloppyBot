namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record FileStorageQuota(
    string ChannelId,
    double MaxStorageQuota,
    int MaxFileNumber,
    double StorageUsed,
    int FileCount);
