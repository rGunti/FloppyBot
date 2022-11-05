namespace FloppyBot.Base.Rng;

public class RandomNumberGenerator : IRandomNumberGenerator
{
    private static readonly Random Rng = new(DateTime.UtcNow.Millisecond + DateTime.Now.Minute);

    public int Next(int min, int max)
    {
        return Rng.Next(min, max);
    }
}
