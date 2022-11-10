using System.Collections;
using System.Collections.Immutable;

namespace FloppyBot.Base.EquatableCollections;

public sealed class ImmutableSetWithValueSemantics<T> : IImmutableSet<T>, IEquatable<IImmutableSet<T>>
{
    private readonly IImmutableSet<T> _set;

    public ImmutableSetWithValueSemantics(IImmutableSet<T> set)
    {
        _set = set;
    }

    public static IImmutableSet<T> Empty => Enumerable.Empty<T>().ToImmutableHashSetWithValueSemantics();
    public bool Equals(IImmutableSet<T>? other) => this.SequenceEqual(other ?? ImmutableHashSet<T>.Empty);

    public override bool Equals(object? obj) => Equals(obj as IImmutableSet<T>);

    public override int GetHashCode()
    {
        unchecked
        {
            return this.Aggregate(19, (h, i) => h * 19 + i!.GetHashCode());
        }
    }

    #region IImmutableSet implementation

    public int Count => _set.Count;

    public IImmutableSet<T> Add(T value) => _set.Add(value).WithValueSemantics();

    public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

    public IImmutableSet<T> Clear() => _set.Clear().WithValueSemantics();

    public bool Contains(T value) => _set.Contains(value);

    public IImmutableSet<T> Except(IEnumerable<T> other) => _set.Except(other).WithValueSemantics();

    public IImmutableSet<T> Intersect(IEnumerable<T> other) => _set.Intersect(other).WithValueSemantics();

    public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

    public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

    public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

    public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

    public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

    public IImmutableSet<T> Remove(T value) => _set.Remove(value).WithValueSemantics();

    public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

    public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other) => _set.SymmetricExcept(other).WithValueSemantics();

    public bool TryGetValue(T equalValue, out T actualValue) => _set.TryGetValue(equalValue, out actualValue);

    public IImmutableSet<T> Union(IEnumerable<T> other) => _set.Union(other).WithValueSemantics();

    IEnumerator IEnumerable.GetEnumerator() => _set.GetEnumerator();

    #endregion IImmutableSet implementation
}
