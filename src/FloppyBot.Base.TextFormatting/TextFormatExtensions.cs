using SmartFormat;

namespace FloppyBot.Base.TextFormatting;

public static class TextFormatExtensions
{
    public static string Format(this string format, object obj)
    {
        return Smart.Format(format, obj);
    }
}
