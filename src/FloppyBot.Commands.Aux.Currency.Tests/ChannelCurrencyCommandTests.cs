using AwesomeAssertions;
using FakeItEasy;
using FloppyBot.Base.Auditing.Abstraction;
using FloppyBot.Base.Storage;
using FloppyBot.Base.Storage.LiteDb;
using FloppyBot.Chat.Entities;
using FloppyBot.Commands.Aux.Currency.Storage;
using FloppyBot.Commands.Aux.Currency.Storage.Entities;

namespace FloppyBot.Commands.Aux.Currency.Tests;

public class ChannelCurrencyCommandTests
{
    private readonly ChannelCurrencyCommands _sut;
    private readonly IRepository<ChannelCurrencySettings> _channelCurrencySettingsRepository;
    private readonly IRepository<ChannelCurrencyRecord> _channelCurrencyRecordRepository;

    public ChannelCurrencyCommandTests()
    {
        var repositoryFactory = LiteDbRepositoryFactory.CreateMemoryInstance();
        _channelCurrencySettingsRepository =
            repositoryFactory.GetRepository<ChannelCurrencySettings>();
        _channelCurrencyRecordRepository = repositoryFactory.GetRepository<ChannelCurrencyRecord>();

        _channelCurrencySettingsRepository.Insert(
            new ChannelCurrencySettings("Mock/Channel", true, "MockCurrency", 100)
        );
        _channelCurrencyRecordRepository.Insert(
            ChannelCurrencyRecord.ForUserInChannel("Mock/Channel", "User", 50)
        );

        _sut = new ChannelCurrencyCommands(
            new ChannelCurrencyService(repositoryFactory),
            A.Fake<IAuditor>(),
            new ChannelCurrencySettingsService(repositoryFactory)
        );
    }

    [Fact]
    public void GetCurrentBalance()
    {
        _sut.GetCurrency("Mock/Channel", new ChatUser("Mock/User", "User", PrivilegeLevel.Viewer))
            .Should()
            .Be(
                "Your current balance is: 50 MockCurrency",
                because: "this user had a stored balance of 50"
            );
    }

    [Fact]
    public void GetCurrentBalanceForNewUser()
    {
        _sut.GetCurrency(
                "Mock/Channel",
                new ChatUser("Mock/UnknownUser", "UnknownUser", PrivilegeLevel.Viewer)
            )
            .Should()
            .Be(
                "Your current balance is: 100 MockCurrency",
                because: "this user did not have a balance yet"
            );
    }

    [Fact]
    public void GetCurrentBalanceForUnconfiguredChannel()
    {
        _sut.GetCurrency(
                "Mock/OtherChannel",
                new ChatUser("Mock/User", "UnknownUser", PrivilegeLevel.Viewer)
            )
            .Should()
            .BeNull(because: "this channel did not have currency configured");
    }

    [Fact]
    public void GiveCurrency()
    {
        _sut.GiveCurrency(
                "Mock/Channel",
                new ChatUser("Mock/User", "User", PrivilegeLevel.Moderator),
                "User",
                "50"
            )
            .Should()
            .Be(
                "The balance has been updated to: 100 MockCurrency",
                because: "this user was given 50 currency"
            );

        _channelCurrencyRecordRepository
            .GetAll()
            .First(b => b is { Channel: "mock/channel", User: "user" })
            .Balance.Should()
            .Be(100, because: "the user now has a balance of 100");
    }

    [Fact]
    public void GiveCurrencyForUnconfiguredChannel()
    {
        _sut.GiveCurrency(
                "Mock/OtherChannel",
                new ChatUser("Mock/User", "User", PrivilegeLevel.Moderator),
                "User",
                "50"
            )
            .Should()
            .BeNull();
    }
}
