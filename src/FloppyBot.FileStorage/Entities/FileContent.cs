using FloppyBot.Base.Storage;

namespace FloppyBot.FileStorage.Entities;

public record FileContent(
        string Id,
        byte[] Content)
    : IEntity<FileContent>
{
    public FileContent WithId(string newId)
    {
        return this with
        {
            Id = newId
        };
    }
}
