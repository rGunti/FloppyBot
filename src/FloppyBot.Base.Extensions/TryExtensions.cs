namespace FloppyBot.Base.Extensions;

public static class TryExtensions
{
    public static T TryOr<T>(Func<T> supplier, Action<Exception> exceptionHandler)
    {
        try
        {
            return supplier();
        }
        catch (Exception ex)
        {
            exceptionHandler(ex);
            throw;
        }
    }

    public static T? TryOr<T>(Func<T> supplier, Func<Exception, T?> exceptionHandler)
    {
        try
        {
            return supplier();
        }
        catch (Exception ex)
        {
            return exceptionHandler(ex);
        }
    }
}
