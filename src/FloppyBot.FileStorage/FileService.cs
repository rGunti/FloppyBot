using FloppyBot.Base.Extensions;
using FloppyBot.Base.Storage;
using FloppyBot.FileStorage.Entities;
using Microsoft.Extensions.Logging;

namespace FloppyBot.FileStorage;

public class FileService : IFileService
{
    private static readonly FileQuota DefaultQuota = new(string.Empty, 15.MegaBytes(), 50);

    private readonly IRepository<FileContent> _fileContent;
    private readonly IRepository<FileHeader> _fileHeaders;
    private readonly IRepository<FileQuota> _fileQuota;

    private readonly ILogger<FileService> _logger;

    public FileService(ILogger<FileService> logger, IRepositoryFactory repositoryFactory)
    {
        _logger = logger;
        _fileHeaders = repositoryFactory.GetRepository<FileHeader>();
        _fileContent = repositoryFactory.GetRepository<FileContent>();
        _fileQuota = repositoryFactory.GetRepository<FileQuota>();
    }

    public IEnumerable<FileHeader> GetFilesOf(string owner)
    {
        _logger.LogDebug("Requesting files for {FileOwner}", owner);
        return _fileHeaders.GetAll().Where(h => h.Owner == owner);
    }

    public NullableObject<FileHeader> GetFile(string owner, string fileName)
    {
        _logger.LogDebug("Requesting file {FileOwner}/{FileName}", owner, fileName);
        return _fileHeaders
            .GetAll()
            .SingleOrDefault(f => f.Owner == owner && f.FileName == fileName)
            .Wrap();
    }

    public bool ExistsFile(string owner, string fileName)
    {
        _logger.LogDebug("Checking for file {FileOwner}/{FileName}", owner, fileName);
        return _fileHeaders.GetAll().Any(f => f.Owner == owner && f.FileName == fileName);
    }

    public Stream GetStreamForFile(string owner, string fileName)
    {
        return GetFile(owner, fileName).SelectMany(header => GetStreamForFile(header.Id)).First();
    }

    public FileQuota GetQuotaFor(string owner)
    {
        _logger.LogDebug("Reading quota for {FileOwner}", owner);
        return _fileQuota.GetById(owner).Wrap().FirstOrDefault()
            ?? DefaultQuota with
            {
                Id = owner,
            };
    }

    public bool CreateFile(string owner, string fileName, string mimeType, Stream fileStream)
    {
        _logger.LogDebug("Storing new file {FileOwner}/{FileName}", owner, fileName);

        if (ExistsFile(owner, fileName))
        {
            _logger.LogWarning("File {FileOwner}/{FileName} does already exist", owner, fileName);
            return false;
        }

        if (!CanClaimStorage(owner, fileStream.Length))
        {
            _logger.LogWarning(
                "New file {FileOwner}/{FileName} did exceed the available quota",
                owner,
                fileName
            );
            return false;
        }

        var ms = new MemoryStream((int)fileStream.Length);
        fileStream.CopyTo(ms);
        byte[] fileContent = ms.GetBuffer();

        _logger.LogTrace("Storing file header {FileOwner}/{FileName}", owner, fileName);
        FileHeader header = _fileHeaders.Insert(
            new FileHeader(
                CreateFileId(owner, fileName),
                owner,
                fileName,
                fileContent.Length,
                mimeType
            )
        );

        _logger.LogTrace("Storing file content {FileOwner}/{FileName}", owner, fileName);
        _fileContent.Insert(new FileContent(header.Id, fileContent));
        return true;
    }

    public void DeleteFile(string owner, string fileName)
    {
        _logger.LogDebug("Deleting file {FileOwner}/{FileName}", owner, fileName);
        string? fileId = GetFile(owner, fileName).Select(f => f.Id).FirstOrDefault();
        if (fileId == null)
        {
            _logger.LogWarning("File {FileOwner}/{FileName} does not exist", owner, fileName);
            return;
        }

        _logger.LogTrace("Deleting file header {FileOwner}/{FileName}", owner, fileName);
        _fileHeaders.Delete(fileId);
        _logger.LogTrace("Deleting file content {FileOwner}/{FileName}", owner, fileName);
        _fileContent.Delete(fileId);
    }

    public bool CanClaimStorage(string owner, double claimedFileSize, int claimedNumberOfFiles = 1)
    {
        _logger.LogDebug(
            "Trying to claim {ClaimFileSize} bytes across {ClaimFileCount} files for {FileOwner}",
            claimedFileSize,
            claimedNumberOfFiles,
            owner
        );
        FileQuota quota = GetQuotaFor(owner);
        FileQuota claimedQuota = GetClaimedQuota(owner);
        FileQuota remainingQuota = quota - claimedQuota;

        return remainingQuota.MaxStorageQuota >= claimedFileSize
            && remainingQuota.MaxFileNumber >= claimedNumberOfFiles;
    }

    public FileQuota GetClaimedQuota(string owner)
    {
        _logger.LogDebug("Calculating claimed quota for {FileOwner}", owner);
        double storedFileSize = GetFilesOf(owner).Select(file => file.FileSize).Sum();
        int fileCount = GetFilesOf(owner).Count();

        return new FileQuota(owner, storedFileSize, fileCount);
    }

    private static string CreateFileId(string owner, string fileName)
    {
        return $"{owner}/{fileName}";
    }

    private IEnumerable<MemoryStream> GetStreamForFile(string fileId)
    {
        _logger.LogDebug("Requesting file content for {FileId}", fileId);
        return _fileContent
            .GetById(fileId)
            .Wrap()
            .Select(content => new MemoryStream(content.Content));
    }
}
