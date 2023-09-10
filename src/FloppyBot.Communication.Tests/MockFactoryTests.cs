using FluentAssertions;

namespace FloppyBot.Communication.Tests;

public class MockFactoryTests
{
    private readonly MockNotificationInterfaceFactory _sut = new();

    [Fact]
    public void CreateSender()
    {
        var sender = _sut.GetNewSender("mock");
        sender.Should().BeOfType<MockNotificationSender>();
    }

    [Fact]
    public void SentMessagesAreRecorded()
    {
        var sender = (MockNotificationSender)_sut.GetNewSender("mock");
        sender.Send(new TestObject("Test User", 42));

        sender.SentMessages.Should().ContainInOrder(new TestObject("Test User", 42));
    }

    [Fact]
    public void CreateReceiver()
    {
        var receiver = _sut.GetNewReceiver<TestObject>("mock");
        receiver.Should().BeOfType<MockNotificationReceiver<TestObject>>();

        ((MockNotificationReceiver<TestObject>)receiver).Channel.Should().Be("mock");
    }
}
