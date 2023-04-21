using FloppyBot.Commands.Aux.UnitConv.Engine.UnitConversion.DTOs;

namespace FloppyBot.Commands.Aux.UnitConv.Tests.UnitConversion
{
    [TestClass]
    public class BasicUnitConversionTests : BaseUnitConversionTest
    {
        protected const float CONVERSION_TOLERANCE = 0.001f;

        [TestMethod]
        [DataRow(2.54f, "cm", 1f, "in")]
        [DataRow(100f, "cm", 1, "m")]
        [DataRow(25f, "km", 25000, "m")]
        [DataRow(0f, "K", -273.15f, "C")]
        [DataRow(0f, "C", 273.15f, "K")]
        [DataRow(0f, "K", -459.67f, "F")]
        [DataRow(0f, "F", 255.372f, "K")]
        [DataRow(1f, "m/s", 3.6f, "km/h")]
        [DataRow(1f, "mph", 1.6092f, "km/h")]
        [DataRow(1f, "pins", 173f, "cm")]
        [DataRow(1f, "kg", 2.20462f, "lb")]
        [DataRow(20f, "gal", 75.708f, "l")]
        [DataRow(100f, "ml", 3.381413f, "floz")]
        public virtual void CanDoSimpleUnitConversions(
            float value,
            string unitName,
            float expectedConvertedValue,
            string convertedUnit,
            float diversionTolerance = CONVERSION_TOLERANCE
        )
        {
            if (!UnitParsingEngine.TryGetUnit(unitName, out var unit))
            {
                Assert.Fail($"Unit <{unitName}> is unknown");
            }

            var unitValue = value.As(unit);

            if (!UnitParsingEngine.TryGetUnit(convertedUnit, out var destinationUnit))
            {
                Assert.Fail($"Unit <{convertedUnit}> is unknown");
            }

            var conversion = UnitConversionEngine.FindConversion(unit, destinationUnit);
            Assert.IsNotNull(
                conversion,
                $"Could not find a conversion from <{unit}> to <{destinationUnit}>"
            );

            Console.WriteLine($"Conversion used: {conversion}");

            var outputValue = conversion.Convert(unitValue);
            var convertDiversion = outputValue - expectedConvertedValue;
            if (Math.Abs(convertDiversion) > diversionTolerance)
            {
                Assert.Fail(
                    $"Conversion from <{unit}> to <{destinationUnit}> resulted in a diversion; "
                        + $"expected <{expectedConvertedValue}>, got <{outputValue}> "
                        + $"(dif <{convertDiversion}>, max <{diversionTolerance}>)\n"
                        + $"Used conversion: <{conversion}>"
                );
            }

            Console.WriteLine($"Expected:        {expectedConvertedValue}");
            Console.WriteLine($"Converted:       {outputValue}");
            Console.WriteLine($"Diversion:       {convertDiversion}");

            var backConversion = UnitConversionEngine.FindConversion(destinationUnit, unit);
            Assert.IsNotNull(
                backConversion,
                $"Could not find a back-conversion from <{destinationUnit}> to <{unit}>"
            );

            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"Back Conversion used: {conversion}");

            var backConvertedOutputValue = backConversion.Convert(outputValue);
            var diversion = value - backConvertedOutputValue;
            if (Math.Abs(diversion) > diversionTolerance)
            {
                Assert.Fail(
                    $"Input value changed when converted back from <{destinationUnit}>! "
                        + $"Expected <{value}>, got <{backConvertedOutputValue}> "
                        + $"(dif <{diversion}>, max <{diversionTolerance}>)\n"
                        + $"Used conversion: <{backConversion}>"
                );
            }

            Console.WriteLine($"Expected:             {value}");
            Console.WriteLine($"Converted:            {backConvertedOutputValue}");
            Console.WriteLine($"Back Diversion:       {diversion}");
        }

        [TestMethod]
        public virtual void CanConvertToSameUnit()
        {
            var failedUnits = new List<Unit>();
            foreach (var unit in UnitParsingEngine.RegisteredUnits)
            {
                var conversion = UnitConversionEngine.FindConversion(unit, unit);
                if (conversion == null)
                {
                    failedUnits.Add(unit);
                }
            }

            if (failedUnits.Any())
            {
                var failedUnitsCount = failedUnits.Count;
                var knownUnitsCount = UnitParsingEngine.RegisteredUnits.Count;

                var failureRate = (double)failedUnitsCount / knownUnitsCount;

                Assert.Fail(
                    $"Same unit conversion failed for "
                        + $"<{failedUnitsCount} / {knownUnitsCount}> known unit(s) (<{failureRate * 100:0.##} %>): "
                        + $"<{string.Join(",", failedUnits.Select(u => u.Symbol).OrderBy(i => i))}>"
                );
            }
        }
    }
}
