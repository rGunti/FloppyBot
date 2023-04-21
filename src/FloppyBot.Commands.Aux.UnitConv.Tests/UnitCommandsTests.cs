using FloppyBot.Commands.Core.Entities;

namespace FloppyBot.Commands.Aux.UnitConv.Tests;

[TestClass]
public class UnitCommandsTests
{
    private readonly UnitCommands _host;

    public UnitCommandsTests()
    {
        _host = new UnitCommands(UnitConvert.DefaultParser, UnitConvert.DefaultConverter);
    }

    [TestMethod]
    public void ConvertUnitAsExpected()
    {
        CommandResult expectedResult = CommandResult.SuccessWith("12 m are about 1200 cm");
        Assert.AreEqual(expectedResult, _host.ConvertUnit("12m", "in", "cm", false));
        Assert.AreEqual(expectedResult, _host.ConvertUnit("12m", "to", "cm", false));
        Assert.AreEqual(expectedResult, _host.ConvertUnit("12m", "cm", null, false));
    }
}
