using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FloppyBot.Base.Configuration.Tests;

[TestClass]
public class AppConfigurationTests
{
    private static IConfiguration BuildTestConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:A", "aConnectionString" },
                { "ConnectionStrings:B", "{A}WithB" },
                { "ConnectionStrings:C", "{A}With\\{SomeValue\\}" },
                { "SomeValue", "CValue" },
                { "SomeOtherValue", "{ConnectionStrings__A}OnRoot" }
            })
            .Build();
    }

    [TestMethod]
    public void ConnectionStringsListedCorrectly()
    {
        IConfiguration config = BuildTestConfig();
        IReadOnlyDictionary<string, string> configStrings = config.GetConnectionStrings();
        CollectionAssert.AreEquivalent(
            new[] { "A", "B", "C" },
            configStrings.Keys.ToArray());
        CollectionAssert.AreEquivalent(
            new[] { "aConnectionString", "{A}WithB", "{A}With\\{SomeValue\\}" },
            configStrings.Values.ToArray());
    }

    [TestMethod]
    public void ConnectionStringsParsedCorrectly()
    {
        IConfiguration config = BuildTestConfig();
        Assert.AreEqual(
            "aConnectionStringWithB",
            config.GetParsedConnectionString("B"));
        Assert.AreEqual(
            "aConnectionStringWith{SomeValue}",
            config.GetParsedConnectionString("C"));
        Assert.AreEqual(
            "aConnectionStringWithCValue",
            config.GetParsedConnectionString("C", true));
    }

    [TestMethod]
    public void ConfigValuesParsedCorrectly()
    {
        IConfiguration config = BuildTestConfig();
        Assert.AreEqual(
            "aConnectionStringOnRoot",
            config.GetParsedConfigString("SomeOtherValue"));
    }
}
