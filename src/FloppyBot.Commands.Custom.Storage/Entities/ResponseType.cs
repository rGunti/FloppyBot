namespace FloppyBot.Commands.Custom.Storage.Entities;

public enum ResponseType
{
    /// <summary>
    /// Text response, visible in chat
    /// </summary>
    Text,

    /// <summary>
    /// An audible sound alert (e.g. played on stream)
    /// </summary>
    Sound,

    [Obsolete("Not yet implemented")]
    JavaScript,

    /// <summary>
    /// A visual alert (e.g. overlay visible on stream)
    /// </summary>
    Visual,
}
