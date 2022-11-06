namespace FloppyBot.WebApi.V1Compatibility.Dtos;

public record FileStorageQuota(
    string ChannelId,
    long MaxStorageQuota,
    int MaxFileNumber,
    long StorageUsed,
    int FileCount);
