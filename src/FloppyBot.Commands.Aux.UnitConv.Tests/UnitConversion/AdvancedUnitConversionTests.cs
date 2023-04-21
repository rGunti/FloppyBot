namespace FloppyBot.Commands.Aux.UnitConv.Tests.UnitConversion
{
    [TestClass]
    public class AdvancedUnitConversionTests : BasicUnitConversionTests
    {
        [TestMethod]
        [DataRow(1f, "km", 1000000f, "mm")]
        [DataRow(1000000f, "mm", 1f, "km")]
        [DataRow(1f, "km", 100000f, "cm")]
        [DataRow(1f, "mi", 5280f, "ft")]
        [DataRow(1f, "mi", 63360f, "in")]
        [DataRow(1f, "mi", 63360f, "ft/in")]
        // [DataRow(1f, "mi", 160934.4f, "cm")]
        // [DataRow(1f, "km", 1093.613f, "yd")]
        // [DataRow(1f, "km", 3280.84f, "ft")]
        [DataRow(1f, "m/s", 2.237136f, "mph")]
        [DataRow(1f, "pins", 1.73f, "m")]
        [DataRow(1f, "pins", 68.11024f, "ft/in")]
        [DataRow(100f, "kg", 15.7473f, "st")]
        [DataRow(1f, "t", 1000000f, "g")]
        [DataRow(10f, "st", 63502.93f, "g")]
        [DataRow(1f, "hl", 100000f, "ml")]
        [DataRow(1f, "l", 1000f, "ml")]
        [DataRow(1f, "dm3", 1f, "l")]
        [DataRow(10f, "l", 0.1f, "m3")]
        [DataRow(1f, "spoh", 1f, "pins")]
        [DataRow(1f, "pins", 1f, "soph")]
        [DataRow(1f, "pins", 1f, "spoh")]
        [DataRow(1f, "spoh", 1f, "soph")]
        public void CanConvertThroughMultipleUnits(
            float value,
            string unitName,
            float expectedConvertedValue,
            string convertedUnit
        )
        {
            CanDoSimpleUnitConversions(
                value,
                unitName,
                expectedConvertedValue,
                convertedUnit,
                0.1f
            );
        }

        [TestMethod]
        public void CanConvertFromKmToYdAndBack() =>
            CanConvertThroughMultipleUnits(1, "km", 1093.613f, "yd");

        [TestMethod]
        public void CanConvertFromKmToFtAndBack() =>
            CanConvertThroughMultipleUnits(1, "km", 3280.84f, "ft");

        [TestMethod]
        public void CanConvertFromMiToCmAndBack() =>
            CanConvertThroughMultipleUnits(1, "mi", 160934.4f, "cm");
    }
}
