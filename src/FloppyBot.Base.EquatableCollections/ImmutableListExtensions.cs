using System.Collections.Immutable;

namespace FloppyBot.Base.EquatableCollections;

public static class ImmutableListExtensions
{
    public static IImmutableList<T> WithValueSemantics<T>(this IImmutableList<T> list)
        => new ImmutableListWithValueSemantics<T>(list);

    public static IImmutableList<T> ToImmutableListWithValueSemantics<T>(this IEnumerable<T> enumerable)
        => enumerable.ToImmutableList().WithValueSemantics();
}
