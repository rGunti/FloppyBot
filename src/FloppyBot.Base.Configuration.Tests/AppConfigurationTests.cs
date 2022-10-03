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
                { "ConnectionStrings:B", "{A}WithB" }
            })
            .Build();
    }
    
    [TestMethod]
    public void ConnectionStringsListedCorrectly()
    {
        IConfiguration config = BuildTestConfig();
        IReadOnlyDictionary<string, string> configStrings = config.GetConnectionStrings();
        CollectionAssert.AreEquivalent(
            new [] { "A", "B" },
            configStrings.Keys.ToArray());
        CollectionAssert.AreEquivalent(
            new [] { "aConnectionString", "{A}WithB" },
            configStrings.Values.ToArray());
    }
    
    [TestMethod]
    public void ConnectionStringsParsedCorrectly()
    {
        IConfiguration config = BuildTestConfig();
        Assert.AreEqual(
            "aConnectionStringWithB",
            config.GetParsedConnectionString("B"));
    }
}
