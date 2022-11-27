using System.Collections;

namespace FloppyBot.Base.Extensions;

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
}
