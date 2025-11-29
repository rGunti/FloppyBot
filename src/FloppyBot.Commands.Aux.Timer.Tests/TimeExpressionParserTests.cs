namespace FloppyBot.Commands.Aux.Timer.Tests;

[TestClass]
public class TimeExpressionParserTests
{
    [TestMethod]
    [DataRow("15s", 15)]
    [DataRow("3m", 3 * 60)]
    [DataRow("2h12m25s", (2 * 60 * 60) + (12 * 60) + 25)]
    public void CanParseTimeExpressions(string input, int expectedNumberOfSeconds)
    {
        Assert.AreEqual(
            TimeSpan.FromSeconds(expectedNumberOfSeconds),
            TimeExpressionParser.ParseTimeExpression(input)
        );
    }
}
