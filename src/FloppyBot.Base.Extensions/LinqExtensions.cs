namespace FloppyBot.Base.Extensions;

public static class LinqExtensions
{
    public static string Join(this IEnumerable<string?> enumerable, string joinWith = "\n")
    {
        return string.Join(joinWith, enumerable);
    }
}
