using FloppyBot.Base.TextFormatting.CustomFormatters;
using SmartFormat;
using SmartFormat.Extensions;

namespace FloppyBot.Base.TextFormatting;

public static class TextFormatExtensions
{
    private static readonly SmartFormatter Formatter = Smart.CreateDefaultSmartFormat()
        .AddExtensions(
            new TimeFormatter(),
            new DateTimeFormatter());

    public static string Format(this string format, object obj)
    {
        return Formatter.Format(format, obj);
    }
}

