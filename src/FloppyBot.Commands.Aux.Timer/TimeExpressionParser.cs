using System.Text.RegularExpressions;

namespace FloppyBot.Commands.Aux.Timer;

public static class TimeExpressionParser
{
    // 12d23h45m12s
    private static readonly Regex TimeParsingRegex = new(
        "^((\\d?\\d)d)?((\\d?\\d)h)?(([0-5]?\\d)m)?(([0-5]?\\d)s)?$",
        RegexOptions.Compiled
    );

    public static TimeSpan? ParseTimeExpression(string expression)
    {
        Match timeParse = TimeParsingRegex.Match(expression);
        if (!timeParse.Success)
        {
            return null;
        }

        string dayStr = timeParse.Groups[2].Value,
            hrsStr = timeParse.Groups[4].Value,
            minStr = timeParse.Groups[6].Value,
            secStr = timeParse.Groups[8].Value;

        int? day = ParseInt(dayStr),
            hrs = ParseInt(hrsStr),
            min = ParseInt(minStr),
            sec = ParseInt(secStr);

        var timeSpan = default(TimeSpan);
        if (day.HasValue)
        {
            timeSpan += TimeSpan.FromDays(day.Value);
        }

        if (hrs.HasValue)
        {
            timeSpan += TimeSpan.FromHours(hrs.Value);
        }

        if (min.HasValue)
        {
            timeSpan += TimeSpan.FromMinutes(min.Value);
        }

        if (sec.HasValue)
        {
            timeSpan += TimeSpan.FromSeconds(sec.Value);
        }

        return timeSpan;
    }

    private static int? ParseInt(string s) => string.IsNullOrWhiteSpace(s) ? null : int.Parse(s);
}
