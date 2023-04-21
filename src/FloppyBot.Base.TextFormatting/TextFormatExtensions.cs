using System.Globalization;
using System.Text;
using FloppyBot.Base.TextFormatting.CustomFormatters;
using SmartFormat;
using SmartFormat.Extensions;

namespace FloppyBot.Base.TextFormatting;

public static class TextFormatExtensions
{
    private static readonly SmartFormatter Formatter = Smart
        .CreateDefaultSmartFormat()
        .AddExtensions(new TimeFormatter(), new DateTimeFormatter());

    public static string Format(this string format, object obj)
    {
        return Formatter.Format(format, obj);
    }

    public static string Join(IEnumerable<string> strings, string separator = " ")
    {
        return string.Join(separator, strings);
    }

    public static string ConvertEmojiSequence(this string hexSequence)
    {
        if (hexSequence.Length % 2 != 0)
        {
            throw new ArgumentException("HEX sequence has to have an even number of characters");
        }

        return Encoding.UTF8.GetString(
            hexSequence
                .SplitIntoChunksOf(2)
                .Select(hex => byte.Parse(hex, NumberStyles.HexNumber))
                .ToArray()
        );
    }

    private static IEnumerable<string> SplitIntoChunksOf(this string s, int chunkSize)
    {
        return Enumerable
            .Range(0, s.Length / chunkSize)
            .Select(i => s.Substring(i * chunkSize, chunkSize));
    }
}
