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
    public static async Task Until(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = new Stopwatch();
        while (!condition())
        {
            await Task.Delay(10);
            if (stopwatch.Elapsed > timeout)
            {
                throw new TimeoutException("Timed out waiting for condition.");
            }
        }
    }

    public static async Task UntilEventIsRaised(Action<WaitArgs> subscribeEvent, TimeSpan timeout)
    {
        var args = new WaitArgs();
        subscribeEvent(args);
        await Until(() => args.Raised, timeout);
    }

    public static async Task DoAndWaitUntil(Action<WaitArgs> waitUntil, Action fn, TimeSpan timeout)
    {
        var args = new WaitArgs();
        waitUntil(args);
        fn();
        await Until(() => args.Raised, timeout);
    }
}
