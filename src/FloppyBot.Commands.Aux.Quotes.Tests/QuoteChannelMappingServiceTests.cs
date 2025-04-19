using FloppyBot.Base.Clock;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Commands.Aux.Quotes.Storage;
using FloppyBot.Commands.Aux.Quotes.Storage.Entities;

namespace FloppyBot.Commands.Aux.Quotes.Tests;

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

    [Fact]
    public void GetQuoteChannelMapping()
    {
        _repository.InsertMany(
            new QuoteChannelMapping(null!, "AnMappingId", "Mock/Channel", true),
            new QuoteChannelMapping(null!, "AnMappingId", "Mock/OtherChannel", true),
            new QuoteChannelMapping(null!, "AnMappingId", "Mock/AdditionalChannel", false)
        );

        Assert.Equal("AnMappingId", _service.GetQuoteChannelMapping("Mock/Channel"));
        Assert.Equal("AnMappingId", _service.GetQuoteChannelMapping("Mock/OtherChannel"));
        Assert.Null(_service.GetQuoteChannelMapping("Mock/AdditionalChannel"));
        Assert.Null(_service.GetQuoteChannelMapping("Mock/AnotherAdditionalChannel"));
    }

    [Fact]
    public void GetQuoteChannelMappingCreatesConfirmedMapping()
    {
        var mappingId = _service.GetQuoteChannelMapping("Mock/Channel", true);
        Assert.Contains(
            _repository.GetAll(),
            m => m.MappingId == mappingId && m.ChannelId == "Mock/Channel" && m.Confirmed
        );
    }

    [Fact]
    public void PositiveJoinProcess()
    {
        // Create first channel mapping
        var mappingId = CreateMapping("Mock/ChannelId");

        // Ask for second channel to join
        var confirmCode = _service.StartJoinProcess(mappingId, "Mock/Channel2");
        Assert.NotNull(confirmCode);

        // Ensure record has been created
        Assert.Contains(
            _repository.GetAll(),
            m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && !m.Confirmed
        );
        Assert.Contains(
            _joinKeyRepo.GetAll(),
            k => k.Id == confirmCode && k.MappingId == mappingId && k.ChannelId == "Mock/Channel2"
        );

        // Advance time a bit
        _timeProvider.AdvanceTimeBy(1.Minute());

        // Confirm with code given
        Assert.True(_service.ConfirmJoinProcess(mappingId, "Mock/Channel2", confirmCode));
        Assert.Contains(
            _repository.GetAll(),
            m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && m.Confirmed
        );
        Assert.DoesNotContain(
            _joinKeyRepo.GetAll(),
            k => k.Id == confirmCode && k.MappingId == mappingId && k.ChannelId == "Mock/Channel2"
        );
    }

    [Fact]
    public void ExpiredJoinProcess()
    {
        // Create first channel mapping
        var mappingId = CreateMapping("Mock/ChannelId");

        // Ask for second channel to join
        var confirmCode = _service.StartJoinProcess(mappingId, "Mock/Channel2");
        Assert.NotNull(confirmCode);

        // Advance time a bit
        _timeProvider.AdvanceTimeBy(6.Minute());

        // Confirm with code given
        Assert.False(_service.ConfirmJoinProcess(mappingId, "Mock/Channel2", confirmCode));
        Assert.DoesNotContain(
            _repository.GetAll(),
            m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId
        );
        Assert.DoesNotContain(
            _joinKeyRepo.GetAll(),
            k => k.Id == confirmCode && k.MappingId == mappingId && k.ChannelId == "Mock/Channel2"
        );
    }

    [Fact]
    public void InvalidKeyJoinProcess()
    {
        // Create first channel mapping
        var mappingId = CreateMapping("Mock/ChannelId");

        // Ask for second channel to join
        var confirmCode = _service.StartJoinProcess(mappingId, "Mock/Channel2");
        Assert.NotNull(confirmCode);

        // Advance time a bit
        _timeProvider.AdvanceTimeBy(1.Minute());

        // Confirm with code given
        Assert.False(_service.ConfirmJoinProcess(mappingId, "Mock/Channel2", "InvalidCode"));
        Assert.DoesNotContain(
            _repository.GetAll(),
            m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && m.Confirmed
        );
        // Key is kept
        Assert.Contains(
            _joinKeyRepo.GetAll(),
            k => k.Id == confirmCode && k.MappingId == mappingId && k.ChannelId == "Mock/Channel2"
        );
    }

    [Fact]
    public void MissingChannelJoinProcess()
    {
        // Create first channel mapping
        var mappingId = CreateMapping("Mock/ChannelId");

        // Ask for second channel to join
        var confirmCode = _service.StartJoinProcess(mappingId, "Mock/Channel2");
        Assert.NotNull(confirmCode);

        // Advance time a bit
        _timeProvider.AdvanceTimeBy(1.Minute());

        // Confirm with code given
        Assert.False(
            _service.ConfirmJoinProcess(mappingId, "Mock/NotTheRightChannel", confirmCode)
        );
        Assert.DoesNotContain(
            _repository.GetAll(),
            m => m.ChannelId == "Mock/Channel2" && m.MappingId == mappingId && m.Confirmed
        );
        // Key is kept
        Assert.Contains(
            _joinKeyRepo.GetAll(),
            k => k.Id == confirmCode && k.MappingId == mappingId && k.ChannelId == "Mock/Channel2"
        );
    }

    private string CreateMapping(string channelId)
    {
        return _service.GetQuoteChannelMapping(channelId, true)!;
    }
}
