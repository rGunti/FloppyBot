namespace FloppyBot.Commands.Aux.UnitConv.Tests.UnitConversion
{
    [TestClass]
    public class BasicUnitParsingTests : BaseUnitConversionTest
    {
        [TestMethod]
        // Distances
        // - Metric
        [DataRow("25.4mm", "mm", 25.4f)]
        [DataRow("185cm", "cm", 185f)]
        [DataRow("250m", "m", 250f)]
        [DataRow("6.5km", "km", 6.5f)]
        // - Imperial
        [DataRow("21in", "in", 21f)]
        [DataRow("6ft1in", "ft/in", 73f)]
        [DataRow("10ft", "ft", 10f)]
        [DataRow("150yd", "yd", 150f)]
        [DataRow("25.7mi", "mi", 25.7f)]
        // Temperature
        [DataRow("25C", "C", 25f)]
        [DataRow("75F", "F", 75f)]
        [DataRow("25°C", "C", 25f)]
        [DataRow("75°F", "F", 75f)]
        [DataRow("250K", "K", 250f)]
        // Speed
        [DataRow("120kmh", "km/h", 120f)]
        [DataRow("120km/h", "km/h", 120f)]
        [DataRow("70mph", "mph", 70f)]
        [DataRow("2.5m/s", "m/s", 2.5f)]
        // Weight
        [DataRow("10kg", "kg", 10f)]
        [DataRow("12st", "st", 12f)]
        [DataRow("100lb", "lb", 100f)]
        [DataRow("123t", "t", 123f)]
        // Volume
        [DataRow("25ml", "ml", 25f)]
        [DataRow("5.4l", "l", 5.4f)]
        [DataRow("25floz", "floz", 25f)]
        [DataRow("69gal", "gal", 69f)]
        public void CanParseKnownUnits(string input, string expectedUnitSymbol, float expectedOutput)
        {
            var val = _unitParsingEngine.ParseUnit(input);
            Assert.IsNotNull(val);
            Assert.AreEqual(expectedOutput, val.Value);
            Assert.AreEqual(expectedUnitSymbol, val.Unit.Symbol);
        }

        [TestMethod]
        // Fakes
        [DataRow("18f", DisplayName = "Fahrenheit must be uppercase")]
        [DataRow("25c", DisplayName = "Celsius must be uppercase")]
        [DataRow("19k", DisplayName = "Kelvin must be uppercase")]
        [DataRow("17y", DisplayName = "Yard is not abbreviated with \"y\"")]
        public void DontParseUnknownUnits(string input)
        {
            var val = _unitParsingEngine.ParseUnit(input);
            Assert.IsNull(val);
        }
    }
}
