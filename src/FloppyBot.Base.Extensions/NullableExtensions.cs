namespace FloppyBot.Base.Extensions;

public static class NullableExtensions
{
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
        return enumerable
            .SingleOrDefault()
            .Wrap();
    }
}
