using System.Collections.Immutable;

namespace FloppyBot.Base.EquatableCollections;

public static class ImmutableListExtensions
{
    public static IImmutableList<T> WithValueSemantics<T>(this IImmutableList<T> list)
        => new ImmutableListWithValueSemantics<T>(list);

    public static IImmutableList<T> ToImmutableListWithValueSemantics<T>(this IEnumerable<T> enumerable)
        => enumerable.ToImmutableList().WithValueSemantics();

    public static IImmutableSet<T> WithValueSemantics<T>(this IImmutableSet<T> set)
        => new ImmutableSetWithValueSemantics<T>(set);

    public static IImmutableSet<T> ToImmutableHashSetWithValueSemantics<T>(this IEnumerable<T> enumerable)
        => enumerable.ToImmutableHashSet().WithValueSemantics();

    public static IImmutableSet<T> ToImmutableSortedSetWithValueSemantics<T>(this IEnumerable<T> enumerable)
        => enumerable.ToImmutableSortedSet().WithValueSemantics();
}
