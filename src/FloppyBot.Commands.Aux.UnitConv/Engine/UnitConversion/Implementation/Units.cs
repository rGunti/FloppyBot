using System.Linq.Expressions;
using System.Text.RegularExpressions;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Abstraction;
using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Conversion;

namespace FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.Implementation;

internal static class Units
{
    public static readonly DTOs.Unit[] AllUnits =
    {
        // Distances
        // - Metric
        ConstructDefaultUnit(UNIT_MM, "millimetre", allowNegative: false),
        ConstructDefaultUnit(UNIT_CM, "centimetre", allowNegative: false),
        ConstructDefaultUnit(UNIT_M, "metre", allowNegative: false),
        ConstructDefaultUnit(UNIT_KM, "kilometre", allowNegative: false),

        // - Imperial
        ConstructDefaultUnit(UNIT_INCH, "inch", allowNegative: false),
        ConstructDefaultUnit(UNIT_FT, "foot", allowNegative: false),
        new(UNIT_FT_INCH, "feet / inches",
            "^((\\d){1,}ft)(\\d{0,}(\\.?\\d{1,})?)in$",
            (_, m) =>
            {
                var match = m[0];
                var ftStr = match.Groups[2];
                var inStr = match.Groups[3];

                var ft = ftStr.Success ? int.Parse(ftStr.Value) : 0;
                var inches = inStr.Success ? int.Parse(inStr.Value) : 0;
                return inches + (ft * 12);
            },
            inches =>
            {
                var feet = Math.Floor(inches / 12);
                var restInches = inches % 12;
                return $"{feet} ft {restInches:0.##} in";
            }),
        ConstructDefaultUnit(UNIT_YD, "yard", allowNegative: false),
        ConstructDefaultUnit(UNIT_MI, "mile", allowNegative: false),

        // - Fun
        ConstructDefaultUnit(UNIT_PINS, "pins length", allowNegative: false),
        ConstructDefaultUnit(UNIT_SOPH, "soph length", allowNegative: false),
        ConstructDefaultUnit(UNIT_SPOH, "spoh length", allowNegative: false),

        // Temperature
        ConstructSuffixedUnit(UNIT_CELSIUS, "Degrees Celsius", "°?C"),
        ConstructSuffixedUnit(UNIT_FAHRENHEIT, "Degrees Fahrenheit", "°?F"),
        ConstructDefaultUnit(UNIT_KELVIN, "Kelvin"),

        // Speed
        ConstructSuffixedUnit(UNIT_KMH, "Kilometres per hour", "km\\/?h", false),
        ConstructDefaultUnit(UNIT_MPH, "Miles per hour", false),
        ConstructDefaultUnit(UNIT_MPS, "Meters per second", false),

        // Weight
        // - Metric
        ConstructDefaultUnit(UNIT_G, "gram", false),
        ConstructDefaultUnit(UNIT_KG, "kilogram", false),
        ConstructDefaultUnit(UNIT_T, "tonne", false),

        // - Imperial
        ConstructDefaultUnit(UNIT_LB, "pound", false),
        ConstructDefaultUnit(UNIT_ST, "stone", false),

        // Volume
        // - Metric
        ConstructDefaultUnit(UNIT_HL, "Hectolitre", false),
        ConstructDefaultUnit(UNIT_L, "Litre", false),
        ConstructDefaultUnit(UNIT_DL, "Decilitre", false),
        ConstructDefaultUnit(UNIT_CL, "Centilitre", false),
        ConstructDefaultUnit(UNIT_ML, "Millilitre", false),

        ConstructSuffixedUnit(UNIT_M3, "Cubic metre", "m[³3]", false,
            customFormatMethod: v => $"{v:0.##} m³"),
        ConstructSuffixedUnit(UNIT_DM3, "Cubic decimetre", "dm[³3]", false,
            customFormatMethod: v => $"{v:0.##} dm³"),
        ConstructSuffixedUnit(UNIT_MM3, "Cubic millimetre", "mm[³3]", false,
            customFormatMethod: v => $"{v:0.##} mm³"),

        // - Imperial
        ConstructDefaultUnit(UNIT_FLOZ, "fluid ounce (US)", false,
            customFormatMethod: v => $"{v:0.##} fl oz"),
        ConstructDefaultUnit(UNIT_GAL, "gallon (US)", false,
            customFormatMethod: v => $"{v:0.##} gal"),
        ConstructDefaultUnit(UNIT_CUP, "cup (US)", false),
        ConstructDefaultUnit(UNIT_TBS, "tablespoon (US)", false),
        ConstructDefaultUnit(UNIT_TSP, "teaspoon (US)", false)
    };

    public static readonly DTOs.Unit DefaultUnit = new(null, "Default Unit", null, null);

    public static readonly Dictionary<(string, string), IUnitConversion> AllConversions;
    public static readonly Dictionary<(string, string), (string, string)[]> AllProxyConversions;

    static Units()
    {
        AllConversions = new Dictionary<(string from, string to), IUnitConversion>()
        {
            // Distances
            // - Metric <-> Metric
            { (UNIT_KM, UNIT_M), Factor(1000f) },
            { (UNIT_M, UNIT_CM), Factor(100f) },
            { (UNIT_CM, UNIT_MM), Factor(10f) },

            // - Imperial <-> Imperial
            { (UNIT_FT_INCH, UNIT_INCH), Same() },
            { (UNIT_MI, UNIT_YD), Factor(1760f) },
            { (UNIT_YD, UNIT_FT), Factor(3f) },
            { (UNIT_FT, UNIT_INCH), Factor(12f) },

            // - Imperial <-> Metrical
            { (UNIT_MI, UNIT_KM), Factor(1.609344f) },
            { (UNIT_YD, UNIT_M), Factor(0.9144f) },
            { (UNIT_FT, UNIT_M), Factor(0.3048f) },
            { (UNIT_INCH, UNIT_CM), Factor(2.54f) },

            // - Fun
            { (UNIT_PINS, UNIT_CM), Factor(173f) },
            { (UNIT_SOPH, UNIT_CM), Factor(160f) },
            { (UNIT_SOPH, UNIT_PINS), Same() },
            { (UNIT_SOPH, UNIT_SPOH), Same() },

            // Temperatures
            { (UNIT_KELVIN, UNIT_CELSIUS), Offset(-273.15f) },
            {
                (UNIT_KELVIN, UNIT_FAHRENHEIT), Formula(
                    k => k * (9 / 5f) - 459.67f,
                    f => (f + 459.67f) * (5 / 9f))
            },
            {
                (UNIT_CELSIUS, UNIT_FAHRENHEIT), Formula(
                    c => c * 1.8f + 32,
                    f => (f - 32) / 1.8f)
            },

            // Speed
            { (UNIT_MPH, UNIT_KMH), Factor(1.609344f) },
            { (UNIT_MPS, UNIT_KMH), Factor(3.6f) },

            // Weight
            // - Metric
            { (UNIT_T, UNIT_KG), Factor(1000f) },
            { (UNIT_KG, UNIT_G), Factor(1000f) },

            // - Imperial
            { (UNIT_ST, UNIT_LB), Factor(14f) },

            // - Imperial <-> Metrical
            { (UNIT_KG, UNIT_LB), Factor(2.20462f) },

            // Volume
            // - Metric
            { (UNIT_HL, UNIT_L), Factor(100f) },
            { (UNIT_L, UNIT_DL), Factor(10f) },
            { (UNIT_DL, UNIT_CL), Factor(10f) },
            { (UNIT_CL, UNIT_ML), Factor(10f) },

            { (UNIT_M3, UNIT_DM3), Factor(1000f) },
            { (UNIT_DM3, UNIT_MM3), Factor(1000000f) },

            { (UNIT_L, UNIT_DM3), Same() },

            // - Imperial
            { (UNIT_GAL, UNIT_FLOZ), Factor(128f) },
            { (UNIT_GAL, UNIT_CUP), Factor(16f) },
            { (UNIT_FLOZ, UNIT_TBS), Factor(2f) },
            { (UNIT_TBS, UNIT_TSP), Factor(3f) },

            // - Imperial <-> Metrical
            { (UNIT_FLOZ, UNIT_ML), Factor(29.5735295625f) },
            { (UNIT_GAL, UNIT_L), Factor(3.785411784f) }
        };

        AllProxyConversions = new Dictionary<(string, string), (string, string)[]>
        {
            { (UNIT_KM, UNIT_MM), Chain(UNIT_KM, UNIT_M, UNIT_CM, UNIT_MM) },
            { (UNIT_MI, UNIT_INCH), Chain(UNIT_MI, UNIT_YD, UNIT_FT, UNIT_INCH, UNIT_FT_INCH) },
            { (UNIT_MPS, UNIT_MPH), Chain(UNIT_MPS, UNIT_KMH, UNIT_MPH) },
            { (UNIT_T, UNIT_G), Chain(UNIT_T, UNIT_KG, UNIT_G) },
            { (UNIT_HL, UNIT_ML), Chain(UNIT_HL, UNIT_L, UNIT_DL, UNIT_CL, UNIT_ML) },
            // Meme Assist: make sure that 1pins=1soph
            { (UNIT_PINS, UNIT_M), Chain(UNIT_PINS, UNIT_CM, UNIT_M) },
            { (UNIT_PINS, UNIT_INCH), Chain(UNIT_PINS, UNIT_CM, UNIT_INCH) },
            { (UNIT_PINS, UNIT_FT_INCH), Chain(UNIT_PINS, UNIT_CM, UNIT_INCH, UNIT_FT_INCH) },
            { (UNIT_SPOH, UNIT_PINS), Chain(UNIT_SPOH, UNIT_SOPH, UNIT_PINS) },
        };
    }

    private static DTOs.Unit ConstructDefaultUnit(
        string unit,
        string unitName,
        bool allowNegative = true,
        bool allowDecimal = true,
        Func<float, string>? customFormatMethod = null)
    {
        var formatMethod = customFormatMethod ?? (Func<float, string>)
            (allowDecimal ? v => $"{v:0.##} {unit}" : v => $"{v:0} {unit}");

        return new DTOs.Unit(unit, unitName,
            GetDefaultNumberRegex(unit, allowNegative, allowDecimal),
            (_, m) => float.Parse(m[0].Groups[1].Value),
            formatMethod);
    }

    private static DTOs.Unit ConstructSuffixedUnit(
        string unit,
        string unitName,
        string suffixRegex,
        bool allowNegative = true,
        bool allowDecimal = true,
        Func<float, string>? customFormatMethod = null)
    {
        var formatMethod = customFormatMethod ?? (Func<float, string>)
            (allowDecimal ? v => $"{v:0.##} {unit}" : v => $"{v:0} {unit}");
        return new DTOs.Unit(unit, unitName,
            GetSuffixedNumberRegex(suffixRegex, allowNegative, allowDecimal),
            (_, m) => float.Parse(m[0].Groups[1].Value),
            formatMethod);
    }

    private static string GetDefaultNumberRegex(
        string unit,
        bool allowNegative = true,
        bool allowDecimal = true)
    {
        var r = "^(";
        if (allowNegative) r += "-?";
        r += "\\d{1,}";
        if (allowDecimal) r += "(\\.?\\d{1,})?";
        r += $"){Regex.Escape(unit)}$";
        return r;
    }

    private static string GetSuffixedNumberRegex(
        string suffixRegex,
        bool allowNegative = true,
        bool allowDecimal = true)
    {
        var r = "^(";
        if (allowNegative) r += "-?";
        r += "\\d{1,}";
        if (allowDecimal) r += "(\\.?\\d{1,})?";
        r += $"){suffixRegex}$";
        return r;
    }

    private static IUnitConversion Same() => new NoneConversion();
    private static IUnitConversion Factor(float factor) => new FactorBasedUnitConversion(factor);
    private static IUnitConversion Offset(float offset) => new OffsetUnitConversion(offset);

    private static IUnitConversion Formula(Expression<Func<float, float>> to, Expression<Func<float, float>> from)
        => new FormulaUnitConversion(to, from);

    private static IUnitConversion Lambda(Func<float, float> to, Func<float, float> from)
        => new LambdaUnitConversion(to, from);

    private static IUnitConversion Invert(IUnitConversion conversion) => new InvertedUnitConversion(conversion);

    private static IUnitConversion Chain(params IUnitConversion[] conversions)
        => new ChainedUnitConversion(conversions);

    private static (string from, string to)[] Chain(params string[] steps)
    {
        var stepPairs = new List<(string from, string to)>();
        for (var i = 0; i < steps.Length - 1; i++)
            stepPairs.Add((steps[i], steps[i + 1]));
        return stepPairs.ToArray();
    }

    #region Unit Names

    // Distances
    // - Metric
    private const string UNIT_MM = "mm";
    private const string UNIT_CM = "cm";
    private const string UNIT_M = "m";
    private const string UNIT_KM = "km";

    // - Imperial
    private const string UNIT_INCH = "in";
    private const string UNIT_FT_INCH = "ft/in";
    private const string UNIT_FT = "ft";
    private const string UNIT_YD = "yd";
    private const string UNIT_MI = "mi";

    // - Fun
    private const string UNIT_PINS = "pins";
    private const string UNIT_SOPH = "soph";
    private const string UNIT_SPOH = "spoh";

    // Temperature
    private const string UNIT_FAHRENHEIT = "F";
    private const string UNIT_CELSIUS = "C";
    private const string UNIT_KELVIN = "K";

    // Speed
    private const string UNIT_KMH = "km/h";
    private const string UNIT_MPS = "m/s";
    private const string UNIT_MPH = "mph";

    // Weight
    // - Metric
    private const string UNIT_G = "g";
    private const string UNIT_KG = "kg";
    private const string UNIT_T = "t";

    // - Imperial
    private const string UNIT_LB = "lb";
    private const string UNIT_ST = "st";

    // Volume
    // - Metric
    private const string UNIT_HL = "hl";
    private const string UNIT_L = "l";
    private const string UNIT_DL = "dl";
    private const string UNIT_CL = "cl";
    private const string UNIT_ML = "ml";
    private const string UNIT_M3 = "m3";
    private const string UNIT_DM3 = "dm3";
    private const string UNIT_MM3 = "mm3";

    // - Imperial
    private const string UNIT_FLOZ = "floz";
    private const string UNIT_GAL = "gal";
    private const string UNIT_CUP = "cup";
    private const string UNIT_TBS = "tbs";
    private const string UNIT_TSP = "tsp";

    #endregion
}


