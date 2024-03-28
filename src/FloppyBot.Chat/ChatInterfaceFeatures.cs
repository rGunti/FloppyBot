namespace FloppyBot.Chat;

[Flags]
public enum ChatInterfaceFeatures
{
    /// <summary>
    /// Default flag value. This chat interface supports no special features.
    /// </summary>
    None = 0,

    /// <summary>
    /// This chat interface supports multiline messages.
    /// </summary>
    Newline = 1,

    /// <summary>
    /// This chat interface supports markdown text formatting.
    /// </summary>
    MarkdownText = 2,
}
