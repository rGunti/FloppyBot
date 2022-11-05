using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Tests;

[TestClass]
public class QuoteChannelMappingServiceTests
{
    private readonly IRepository<QuoteChannelMappingJoinKeys> _joinKeyRepo;
    private readonly IRepository<QuoteChannelMapping> _repository;
    private readonly QuoteChannelMappingService _service;
    private readonly FixedTimeProvider _timeProvider;

    public QuoteChannelMappingServiceTests()
    {
        var factory = LiteDbRepositoryFactory.CreateMemoryInstance();
        _repository = factory.GetRepository<QuoteChannelMapping>();
        _joinKeyRepo = factory.GetRepository<QuoteChannelMappingJoinKeys>();
        _timeProvider = new FixedTimeProvider();
        _service = new QuoteChannelMappingService(factory, _timeProvider);
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

    private string CreateMapping(string channelId)
    {
        return _service.GetQuoteChannelMapping(channelId, true)!;
    }

    [TestMethod]
    public void PositiveJoinProcess()
    {
        // Create first channel mapping
        var mappingId = CreateMapping("Mock/ChannelId");

        // Ask for second channel to join
        var confirmCode = _service.StartJoinProcess(mappingId, "Mock/Channel2");
        Assert.IsNotNull(confirmCode);

        // Ensure record has been created
        Assert.IsTrue(_repository.GetAll()
            .Any(m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && !m.Confirmed));
        Assert.IsTrue(_joinKeyRepo.GetAll()
            .Any(k => k.Id == confirmCode
                      && k.MappingId == mappingId
                      && k.ChannelId == "Mock/Channel2"));

        // Advance time a bit
        _timeProvider.AdvanceTimeBy(1.Minute());

        // Confirm with code given
        Assert.IsTrue(_service.ConfirmJoinProcess(mappingId, "Mock/Channel2", confirmCode));
        Assert.IsTrue(_repository.GetAll()
            .Any(m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && m.Confirmed));
        Assert.IsFalse(_joinKeyRepo.GetAll()
            .Any(k => k.Id == confirmCode
                      && k.MappingId == mappingId
                      && k.ChannelId == "Mock/Channel2"));
    }

    [TestMethod]
    public void ExpiredJoinProcess()
    {
        // Create first channel mapping
        var mappingId = CreateMapping("Mock/ChannelId");

        // Ask for second channel to join
        var confirmCode = _service.StartJoinProcess(mappingId, "Mock/Channel2");
        Assert.IsNotNull(confirmCode);

        // Advance time a bit
        _timeProvider.AdvanceTimeBy(6.Minute());

        // Confirm with code given
        Assert.IsFalse(_service.ConfirmJoinProcess(mappingId, "Mock/Channel2", confirmCode));
        Assert.IsFalse(_repository.GetAll()
            .Any(m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && m.Confirmed));
        Assert.IsFalse(_joinKeyRepo.GetAll()
            .Any(k => k.Id == confirmCode
                      && k.MappingId == mappingId
                      && k.ChannelId == "Mock/Channel2"));
    }

    [TestMethod]
    public void InvalidKeyJoinProcess()
    {
        // Create first channel mapping
        var mappingId = CreateMapping("Mock/ChannelId");

        // Ask for second channel to join
        var confirmCode = _service.StartJoinProcess(mappingId, "Mock/Channel2");
        Assert.IsNotNull(confirmCode);

        // Advance time a bit
        _timeProvider.AdvanceTimeBy(1.Minute());

        // Confirm with code given
        Assert.IsFalse(_service.ConfirmJoinProcess(mappingId, "Mock/Channel2", "InvalidCode"));
        Assert.IsFalse(_repository.GetAll()
            .Any(m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && m.Confirmed));
        // Key is kept
        Assert.IsTrue(_joinKeyRepo.GetAll()
            .Any(k => k.Id == confirmCode
                      && k.MappingId == mappingId
                      && k.ChannelId == "Mock/Channel2"));
    }

    [TestMethod]
    public void MissingChannelJoinProcess()
    {
        // Create first channel mapping
        var mappingId = CreateMapping("Mock/ChannelId");

        // Ask for second channel to join
        var confirmCode = _service.StartJoinProcess(mappingId, "Mock/Channel2");
        Assert.IsNotNull(confirmCode);

        // Advance time a bit
        _timeProvider.AdvanceTimeBy(1.Minute());

        // Confirm with code given
        Assert.IsFalse(_service.ConfirmJoinProcess(mappingId, "Mock/NotTheRightChannel", confirmCode));
        Assert.IsFalse(_repository.GetAll()
            .Any(m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && m.Confirmed));
        // Key is kept
        Assert.IsTrue(_joinKeyRepo.GetAll()
            .Any(k => k.Id == confirmCode
                      && k.MappingId == mappingId
                      && k.ChannelId == "Mock/Channel2"));
    }
}
