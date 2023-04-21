using SmartFormat.Core.Extensions;

namespace FloppyBot.Base.TextFormatting.CustomFormatters;

public class DateTimeFormatter : IFormatter
{
    public string Name { get; set; } = "datetime";
    public bool CanAutoDetect { get; set; } = true;

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue == null)
        {
            return false;
        }

        var dateTimeOffset = (DateTimeOffset)formattingInfo.CurrentValue;
        string formatStr = formattingInfo.Format!.RawText;
        if (formatStr.StartsWith('\'') && formatStr.EndsWith('\''))
        {
            formatStr = formatStr.Substring(1, formatStr.Length - 2);
        }

        formattingInfo.Write(dateTimeOffset.ToString(formatStr));
        return true;
    }
}
