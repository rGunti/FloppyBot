using System.Collections.Immutable;
using System.Reflection;

namespace FloppyBot.Base.Extensions;

public static class ObjectExtensions
{
    public static ImmutableDictionary<string, object?> AsDictionary(
        this object? source,
        BindingFlags bindingAttr =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance
    )
    {
        if (source == null)
        {
            return ImmutableDictionary<string, object?>.Empty;
        }

        return source
            .GetType()
            .GetProperties(bindingAttr)
            .ToImmutableDictionary(
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
    }
}
