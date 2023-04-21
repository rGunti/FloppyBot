using System.Collections.Immutable;
using FloppyBot.Base.EquatableCollections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FloppyBot.Base.Tests.EquatableCollections;

internal record SampleRecord(string Name, IImmutableList<string> AList);

[TestClass]
public class EquatableCollectionsTests
{
    [TestMethod]
    public void CollectionsAreEqual()
    {
        IImmutableList<string> listA = new[]
        {
            "Hello",
            "World",
        }.ToImmutableListWithValueSemantics();
        IImmutableList<string> listB = new[]
        {
            "Hello",
            "World",
        }.ToImmutableListWithValueSemantics();

        Assert.AreEqual(listA, listB);
        Assert.AreNotEqual(listA, listA.Add("World"));
        Assert.AreNotEqual(listA, listA.Remove("World"));
    }

    [TestMethod]
    public void RecordsAreEqual()
    {
        var recordA = new SampleRecord(
            "A Record",
            new[] { "Hello World" }.ToImmutableListWithValueSemantics()
        );
        var recordB = new SampleRecord(
            "A Record",
            new[] { "Hello World" }.ToImmutableListWithValueSemantics()
        );

        Assert.AreEqual(recordA, recordB);
    }
}
