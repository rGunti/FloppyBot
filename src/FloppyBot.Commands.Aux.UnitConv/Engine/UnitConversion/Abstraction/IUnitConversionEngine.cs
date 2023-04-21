using System.Collections.Immutable;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;

public interface IUnitConversionEngine
{
    IImmutableDictionary<(string From, string To), IUnitConversion> RegisteredConversions { get; }

    bool HasDirectConversion(string from, string to);
    bool HasInvertedConversion(string from, string to);

    IUnitConversion GetDirectConversion(string from, string to);
    IUnitConversion GetInvertedConversion(string from, string to);

    IUnitConversion? FindConversion(string from, string to);
}
