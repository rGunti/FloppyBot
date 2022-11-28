using FloppyBot.Base.Extensions;
using FloppyBot.FileStorage.Entities;

namespace FloppyBot.FileStorage;

public interface IFileService
{
    IEnumerable<FileHeader> GetFilesOf(string owner);
    NullableObject<FileHeader> GetFile(string owner, string fileName);
    bool ExistsFile(string owner, string fileName);
    Stream GetStreamForFile(string owner, string fileName);
    FileQuota GetQuotaFor(string owner);
    bool CreateFile(string owner, string fileName, string mimeType, Stream fileStream);
    void DeleteFile(string owner, string fileName);
    bool CanClaimStorage(string owner, double claimedFileSize, int claimedNumberOfFiles = 1);
    FileQuota GetClaimedQuota(string owner);
}
