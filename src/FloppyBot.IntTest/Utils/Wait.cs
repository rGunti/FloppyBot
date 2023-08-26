using System.Diagnostics;

namespace FloppyBot.IntTest.Utils;

public record WaitArgs
{
    public bool Raised { get; private set; }

    public void DeclareRaised()
    {
        Raised = true;
    }
}

public static class Wait
{
    public static async Task Until(
        Func<bool> condition,
        TimeSpan timeout,
        string? timeoutMessage = null
    )
    {
        using var timer = new Timer();
        while (!condition())
        {
            await Task.Delay(10);
            if (timer.Elapsed > timeout)
            {
                throw new TimeoutException(timeoutMessage ?? "Timed out waiting for condition.");
            }
        }
    }

    public static async Task UntilEventIsRaised(
        Action<WaitArgs> subscribeEvent,
        TimeSpan timeout,
        string? timeoutMessage = null
    )
    {
        var args = new WaitArgs();
        subscribeEvent(args);
        await Until(() => args.Raised, timeout, timeoutMessage);
    }

    public static async Task DoAndWaitUntil(
        Action<WaitArgs> waitUntil,
        Action fn,
        TimeSpan timeout,
        string? timeoutMessage = null
    )
    {
        var args = new WaitArgs();
        waitUntil(args);
        fn();
        await Until(() => args.Raised, timeout, timeoutMessage);
    }
}

public class Timer : IDisposable
{
    private readonly Stopwatch _stopwatch;

    public Timer()
    {
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public void Dispose()
    {
        _stopwatch.Stop();
        GC.SuppressFinalize(this);
    }
}
