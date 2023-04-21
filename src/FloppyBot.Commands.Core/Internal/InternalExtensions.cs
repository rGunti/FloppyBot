using System.Reflection;

namespace FloppyBot.Commands.Core.Internal;

internal static class InternalExtensions
{
    public static bool HasCustomAttribute<T>(this Type type)
        where T : Attribute
    {
        return type.GetCustomAttributes<T>().Any();
    }

    public static bool HasCustomAttribute<T>(this MethodBase method)
        where T : Attribute
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

    public static bool IsType<T>(this Type type)
    {
        return type == typeof(T);
    }

    public static bool IsOfType<T>(this ParameterInfo parameterInfo)
    {
        return parameterInfo.ParameterType.IsType<T>();
    }

    public static void AssertType<T>(this ParameterInfo parameterInfo)
    {
        parameterInfo.ParameterType.AssertType<T>();
    }

    public static void AssertType<T>(this Type type)
    {
        if (type != typeof(T) && !type.IsAssignableFrom(typeof(T)))
        {
            throw new InvalidCastException(
                $"Parameter of type {type} cannot be assigned to {typeof(T)}"
            );
        }
    }
}
