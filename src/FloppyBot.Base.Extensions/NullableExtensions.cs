namespace FloppyBot.Base.Extensions;

public static class NullableExtensions
{
    public static T Or<T>(this NullableObject<T> wrapped, T other)
        where T : class
    {
        return wrapped.HasValue ? wrapped.Value : other;
    }

    public static T Or<T>(this NullableObject<T> wrapped, Func<T> other)
        where T : class
    {
        return wrapped.HasValue ? wrapped.Value : other();
    }

    public static T OrThrow<T>(this T? obj)
    {
        return obj ?? throw new ArgumentNullException(nameof(obj));
    }

    public static T OrThrow<T>(this T? obj, Func<Exception> exceptionSupplier)
    {
        return obj ?? throw exceptionSupplier();
    }

    public static T OrThrow<T>(this NullableObject<T> wrapped)
        where T : class
    {
        return wrapped.HasValue ? wrapped.Value : throw new ArgumentNullException(nameof(wrapped));
    }

    public static T OrThrow<T>(this NullableObject<T> wrapped, Func<Exception> exceptionSupplier)
        where T : class
    {
        return wrapped.HasValue ? wrapped.Value : throw exceptionSupplier();
    }

    public static NullableObject<T> Wrap<T>(this T? obj)
        where T : class
    {
        return new NullableObject<T>(obj);
    }

    public static NullableObject<T> Wrap<T>(this IEnumerable<T> enumerable)
        where T : class
    {
        return enumerable.SingleOrDefault().Wrap();
    }

    public static NullableObject<T> Where<T>(this NullableObject<T> obj, Func<T, bool> predicate)
        where T : class
    {
        if (obj.HasValue && predicate(obj.Value))
        {
            return obj;
        }

        return NullableObject.Empty<T>();
    }

    public static NullableObject<TOutput> Select<TInput, TOutput>(
        this NullableObject<TInput> obj,
        Func<TInput, TOutput> func
    )
        where TInput : class
        where TOutput : class
    {
        return obj.HasValue ? func(obj.Value).Wrap() : NullableObject.Empty<TOutput>();
    }

    public static NullableObject<TOutput> Select<TInput, TOutput>(
        this NullableObject<TInput> obj,
        Func<TInput, NullableObject<TOutput>> func
    )
        where TInput : class
        where TOutput : class
    {
        return obj.HasValue ? func(obj.Value) : NullableObject.Empty<TOutput>();
    }

    public static NullableObject<T> Tap<T>(this NullableObject<T> obj, Action<T> action)
        where T : class
    {
        if (obj.HasValue)
        {
            action(obj.Value);
        }

        return obj;
    }

    public static IEnumerable<T> Tap<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
        {
            action(item);
            yield return item;
        }
    }

    public static void Complete<T>(this NullableObject<T> obj, Action<T> action)
        where T : class
    {
        if (obj.HasValue)
        {
            action(obj.Value);
        }
    }

    public static void Complete<T>(this IEnumerable<T> enumerable, Action<T>? action = null)
        where T : class
    {
        foreach (var item in enumerable)
        {
            action?.Invoke(item);
        }
    }
}
