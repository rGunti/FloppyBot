using FloppyBot.FileStorage.Entities;

namespace FloppyBot.WebApi.V2.Dtos;

public record FileStorageQuotaDto(
    string ChannelId,
    double MaxStorageQuota,
    int MaxFileNumber,
    double StorageUsed,
    int FileCount
)
{
    public static FileStorageQuotaDto FromQuota(
        string channelId,
        FileQuota quota,
        FileQuota usedQuota
    )
    {
        return new FileStorageQuotaDto(
            channelId,
            quota.MaxStorageQuota,
            quota.MaxFileNumber,
            usedQuota.MaxStorageQuota,
            usedQuota.MaxFileNumber
        );
    }
}
