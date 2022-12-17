using FloppyBot.Commands.Aux.Twitch.Storage;
using FloppyBot.FileStorage;

namespace FloppyBot.WebApi.V1Compatibility.DataImport;

internal static class V1DataImportServiceExtensions
{
    internal static Tuple<string, string, string, Stream> CreateFileCall(
        string owner, string fileName, string mimeType, Stream fileStream)
    {
        return Tuple.Create(
            owner,
            fileName,
            mimeType,
            fileStream);
    }

    internal static bool CreateFile(
        this IFileService fileService,
        Tuple<string, string, string, Stream> preparedCall)
    {
        return fileService.CreateFile(
            preparedCall.Item1,
            preparedCall.Item2,
            preparedCall.Item3,
            preparedCall.Item4);
    }

    internal static Tuple<string, string> CreateShoutoutCall(
        string channel, string message)
    {
        return Tuple.Create(channel, message);
    }

    internal static void SetShoutoutMessage(
        this IShoutoutMessageSettingService service,
        Tuple<string, string> preparedCall)
    {
        service.SetShoutoutMessage(
            preparedCall.Item1,
            preparedCall.Item2);
    }
}

