using System.Collections;

namespace FloppyBot.Base.Extensions;

public static class NullableObject
{
    public static NullableObject<T> Empty<T>()
        where T : class
        => new(default);
}

public class NullableObject<T> : IEnumerable<T>
    where T : class
{
    private readonly T? _value;

    public NullableObject(T? value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the actual value wrapped
    /// </summary>
    public T Value => _value.OrThrow(() => new ArgumentNullException());

    /// <summary>
    /// If the wrapped value is not null, this property will hold true
    /// </summary>
    public bool HasValue => _value != null;

    public IEnumerator<T> GetEnumerator()
    {
        if (HasValue)
        {
            yield return _value!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public static implicit operator T(NullableObject<T> wrapper) => wrapper.Value;
    public static implicit operator NullableObject<T>(T obj) => new(obj);
}

