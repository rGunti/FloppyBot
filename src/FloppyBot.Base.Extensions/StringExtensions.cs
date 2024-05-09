namespace FloppyBot.Base.Extensions;

public static class StringExtensions
{
    public static string Capitalize(this string s)
    {
        return s.Length switch
        {
            0 => s,
            1 => char.ToUpperInvariant(s[0]).ToString(),
            _ => $"{char.ToUpperInvariant(s[0])}{s[1..]}",
        };
    }

    public static int ParseInt(this string s, int defaultValue = 0)
    {
        return int.TryParse(s, out var result) ? result : defaultValue;
    }
}
