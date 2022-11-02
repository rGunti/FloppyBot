using System.Collections.Immutable;

namespace FloppyBot.Base.Rng;

public class StaticNumberGenerator : IRandomNumberGenerator
{
    private int _currentIndex;
    private IImmutableList<int> _numbers;


    public StaticNumberGenerator(params int[] numbers)
    {
        _numbers = numbers.ToImmutableList();
    }

    public StaticNumberGenerator(IEnumerable<int> numbers)
    {
        _numbers = numbers.ToImmutableList();
    }

    public int Next(int _, int __)
    {
        return _numbers[_currentIndex += 1 % _numbers.Count];
    }

    public void SetNumbers(params int[] newNumbers)
    {
        _currentIndex = 0;
        _numbers = newNumbers.ToImmutableList();
    }

    public void SetNumbers(IEnumerable<int> newNumbers)
    {
        _currentIndex = 0;
        _numbers = newNumbers.ToImmutableList();
    }
}
