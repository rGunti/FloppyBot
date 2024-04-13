using FloppyBot.FileStorage.Entities;

namespace FloppyBot.WebApi.V2.Dtos;

public record FileHeaderDto(
    string Id,
    string Owner,
    string FileName,
    double FileSize,
    string MimeType
)
{
    public static FileHeaderDto FromFileHeader(FileHeader fileHeader)
    {
        return new FileHeaderDto(
            fileHeader.Id,
            fileHeader.Owner,
            fileHeader.FileName,
            fileHeader.FileSize,
            fileHeader.MimeType
        );
    }
}
