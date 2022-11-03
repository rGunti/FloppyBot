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
        _repository.InsertMany(
            new QuoteChannelMapping(
                null!,
                "AnMappingId",
                "Mock/Channel",
                true),
            new QuoteChannelMapping(
                null!,
                "AnMappingId",
                "Mock/OtherChannel",
                true),
            new QuoteChannelMapping(
                null!,
                "AnMappingId",
                "Mock/AdditionalChannel",
                false));

        Assert.AreEqual("AnMappingId", _service.GetQuoteChannelMapping("Mock/Channel"));
        Assert.AreEqual("AnMappingId", _service.GetQuoteChannelMapping("Mock/OtherChannel"));
        Assert.IsNull(_service.GetQuoteChannelMapping("Mock/AdditionalChannel"));
        Assert.IsNull(_service.GetQuoteChannelMapping("Mock/AnotherAdditionalChannel"));
    }

    [TestMethod]
    public void GetQuoteChannelMappingCreatesConfirmedMapping()
    {
        var mappingId = _service.GetQuoteChannelMapping("Mock/Channel", true);
        Assert.IsTrue(
            _repository.GetAll()
                .Any(m => m.MappingId == mappingId
                          && m.ChannelId == "Mock/Channel"
                          && m.Confirmed));
    }
}
