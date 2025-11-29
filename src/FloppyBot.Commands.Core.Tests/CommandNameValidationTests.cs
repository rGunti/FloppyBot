namespace FloppyBot.Commands.Core.Tests;

[TestClass]
public class CommandNameValidationTests
{
    [TestMethod]
    [DataRow(null, false)]
    [DataRow("", false)]
    [DataRow("ping", true)]
    [DataRow("Ping1", true)]
    [DataRow("some name", false)]
    public void IsValidCommandName(string? input, bool expectValid)
    {
        Assert.AreEqual(expectValid, input.IsValidCommandName());
    }
}
