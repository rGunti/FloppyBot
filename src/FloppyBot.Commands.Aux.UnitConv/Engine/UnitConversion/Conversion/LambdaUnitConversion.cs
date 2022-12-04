using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Conversion;

internal class LambdaUnitConversion : IUnitConversion
{
    private readonly Func<float, float> _backConversionFunc;
    private readonly Func<float, float> _conversionFunc;

    public LambdaUnitConversion(
        Func<float, float> conversionFunc,
        Func<float, float> backConversionFunc)
    {
        _conversionFunc = conversionFunc;
        _backConversionFunc = backConversionFunc;
    }

    public float Convert(float input) => _conversionFunc(input);
    public float ConvertBack(float input) => _backConversionFunc(input);

    public override string ToString()
        => $"Lambda(<native code#{_conversionFunc.Method.MetadataToken}>, <native code#{_backConversionFunc.Method.MetadataToken}>)";
}

