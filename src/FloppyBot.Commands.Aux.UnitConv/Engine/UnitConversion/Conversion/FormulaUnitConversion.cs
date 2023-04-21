using System.Linq.Expressions;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Conversion;

internal class FormulaUnitConversion : IUnitConversion
{
    private readonly Expression<Func<float, float>> _backConversion;
    private readonly Expression<Func<float, float>> _conversion;
    private Func<float, float>? _backConversionFunc;

    private Func<float, float>? _conversionFunc;

    public FormulaUnitConversion(
        Expression<Func<float, float>> conversion,
        Expression<Func<float, float>> backConversion
    )
    {
        _conversion = conversion;
        _backConversion = backConversion;
    }

    public float Convert(float input)
    {
        _conversionFunc ??= _conversion.Compile();
        return _conversionFunc(input);
    }

    public float ConvertBack(float input)
    {
        _backConversionFunc ??= _backConversion.Compile();
        return _backConversionFunc(input);
    }

    public override string ToString() => $"Formula({_conversion}, {_backConversion})";
}
