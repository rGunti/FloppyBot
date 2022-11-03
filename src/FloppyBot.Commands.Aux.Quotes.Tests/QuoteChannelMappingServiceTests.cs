using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Tests;

[TestClass]
public class QuoteChannelMappingServiceTests
{
    private readonly IRepository<QuoteChannelMapping> _repository;
    private readonly QuoteChannelMappingService _service;

    public QuoteChannelMappingServiceTests()
    {
        var factory = LiteDbRepositoryFactory.CreateMemoryInstance();
        _repository = factory.GetRepository<QuoteChannelMapping>();
        _service = new QuoteChannelMappingService(factory);
    }

    [TestMethod]
    public void GetQuoteChannelMapping()
    {
        _repository.Insert(new QuoteChannelMapping(
            "someId",
            new[] { "Mock/Channel", "Mock/OtherChannel" },
            false));

        Assert.AreEqual("someId", _service.GetQuoteChannelMapping("Mock/Channel"));
        Assert.AreEqual("someId", _service.GetQuoteChannelMapping("Mock/OtherChannel"));
        Assert.IsNull(_service.GetQuoteChannelMapping("Mock/AdditionalChannel"));
    }

    [TestMethod]
    public void GetQuoteChannelMappingCreatesMapping()
    {
        Assert.IsFalse(
            _repository.GetAll()
                .Any(m => m.ChannelIds.Contains("Mock/Channel")));

        var mappingId = _service.GetQuoteChannelMapping("Mock/Channel", true);

        Assert.IsTrue(
            _repository.GetAll()
                .Any(m => m.Id == mappingId && m.ChannelIds.Contains("Mock/Channel")));
    }
}
