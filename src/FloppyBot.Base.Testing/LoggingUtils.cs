using Microsoft.Extensions.Logging;
using Moq;

namespace FloppyBot.Base.Testing;

public static class LoggingUtils
{
    public static ILogger<T> GetLogger<T>()
    {
        return Mock.Of<ILogger<T>>();
    }
}
