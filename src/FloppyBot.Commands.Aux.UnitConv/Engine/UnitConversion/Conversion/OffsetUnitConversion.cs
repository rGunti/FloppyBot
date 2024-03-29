﻿using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Conversion;

internal class OffsetUnitConversion : IUnitConversion
{
    public OffsetUnitConversion(float offset)
    {
        Offset = offset;
    }

    public float Offset { get; }

    public float Convert(float input) => input + Offset;

    public float ConvertBack(float input) => input - Offset;

    public override string ToString() => $"Offset({Offset})";
}
