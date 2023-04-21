using FloppyBot.WebApi.V1Compatibility.Dtos;

namespace FloppyBot.WebApi.V1Compatibility.DataImport.Entities;

public record DataExportFile(FileHeader Header, byte[] Content, string CheckSum);
