using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.Attributes;

namespace FloppyBot.FileStorage.Entities;

[IndexFields("Owner_FileName", nameof(Owner), nameof(FileName))]
public record FileHeader(string Id, string Owner, string FileName, double FileSize, string MimeType)
    : IEntity<FileHeader>
{
    public FileHeader WithId(string newId)
    {
        return this with { Id = newId };
    }
}
