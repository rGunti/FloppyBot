using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FloppyBot.Base.Testing;

public static class LoggingUtils
{
    public static ILogger<T> GetLogger<T>()
    {
        return NullLogger<T>.Instance;
    }
}
