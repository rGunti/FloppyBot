using FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.Unit.Engine.UnitConversion.Conversion;

internal class ChainedUnitConversion : IUnitConversion
{
    private readonly IUnitConversion[] _conversions;
    private readonly string _customLabel = null;

    public ChainedUnitConversion(params IUnitConversion[] conversions)
    {
        _conversions = conversions;
    }

    public ChainedUnitConversion(
        IEnumerable<IUnitConversion> conversions,
        string customLabel = null) : this(conversions.ToArray())
    {
        _customLabel = customLabel;
    }

    private string ChainDescription => string.Join(", ", _conversions.Select(i => i.ToString()));

    public float Convert(float input)
    {
        foreach (var conversion in _conversions)
        {
            input = conversion.Convert(input);
        }

        return input;
    }

    public float ConvertBack(float input)
    {
        foreach (var conversion in _conversions.Reverse())
        {
            input = conversion.Convert(input);
        }

        return input;
    }

    public override string ToString()
        => $"Chain({(string.IsNullOrEmpty(_customLabel) ? ChainDescription : _customLabel)})";
}
