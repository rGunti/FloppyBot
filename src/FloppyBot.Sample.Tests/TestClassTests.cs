using FloppyBot.Sample.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FloppyBot.Sample.Tests;

[TestClass]
public class TestClassTests
{
    [TestMethod]
    public void TextIsAsExpected()
    {
        var item = new TestClass("Hello World");
        Assert.AreEqual("Hello World", item.Text);
    }
}
