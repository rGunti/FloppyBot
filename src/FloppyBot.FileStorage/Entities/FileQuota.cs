using FloppyBot.Base.Storage;

namespace FloppyBot.FileStorage.Entities;

public record FileQuota(
        string Id,
        double MaxStorageQuota,
        int MaxFileNumber)
    : IEntity<FileQuota>
{
    public FileQuota WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }

    /// <summary>
    /// Calculates the difference between two quotas.
    /// If a quota is breached (i.e. the value falls below 0),
    /// it be provided as 0. No negative values will be returned.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static FileQuota operator -(FileQuota a, FileQuota b)
    {
        return new FileQuota(
            a.Id,
            Math.Max(a.MaxStorageQuota - b.MaxStorageQuota, 0),
            Math.Max(a.MaxFileNumber - b.MaxFileNumber, 0)
        );
    }
}
