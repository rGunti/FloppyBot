using System.Reflection;

namespace FloppyBot.Commands.Core.Internal;

internal static class InternalExtensions
{
    public static bool HasCustomAttribute<T>(this Type type) where T : Attribute
    {
        return type.GetCustomAttributes<T>().Any();
    }

    public static bool HasCustomAttribute<T>(this MethodBase method) where T : Attribute
    {
        return method.GetCustomAttributes<T>().Any();
    }

    public static IEnumerable<T> Inspect<T>(this IEnumerable<T> enumerable, Action<T> inspectFn)
    {
        foreach (var item in enumerable)
        {
            inspectFn(item);
            yield return item;
        }
    }
}
