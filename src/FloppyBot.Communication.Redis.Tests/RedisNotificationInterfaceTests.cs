using System;
using System.Collections.Generic;
using FloppyBot.Base.Testing;
using FloppyBot.Communication.Redis.Config;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StackExchange.Redis;

namespace FloppyBot.Communication.Redis.Tests;

[TestClass]
public class RedisNotificationInterfaceTests
{
    private readonly RedisNotificationInterfaceFactory _interfaceFactory;
    private readonly Mock<ISubscriber> _subscriberMock;

    public RedisNotificationInterfaceTests()
    {
        _subscriberMock = new Mock<ISubscriber>();

        var connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
        connectionMultiplexerMock
            .Setup(s => s.GetSubscriber(It.IsAny<object?>()))
            .Returns<object?>((asyncState) => _subscriberMock.Object);

        var factory = new Mock<IRedisConnectionFactory>();
        factory
            .Setup(f => f.GetMultiplexer(It.IsAny<RedisConnectionConfig>()))
            .Returns(() => connectionMultiplexerMock.Object);

        _interfaceFactory = new RedisNotificationInterfaceFactory(
            LoggingUtils.GetLogger<RedisNotificationInterfaceFactory>(),
            factory.Object
        );
    }

    [TestMethod]
    public void ReceiverIsCreatedCorrectly()
    {
        INotificationReceiver<object> receiver = _interfaceFactory.GetNewReceiver<object>(
            "redis.host.invalid|SomeChannel"
        );
        Assert.IsInstanceOfType(receiver, typeof(RedisNotificationReceiver<object>));
        Assert.AreEqual("SomeChannel", ((RedisNotificationReceiver<object>)receiver).Channel);
    }

    [TestMethod]
    public void ReceiverIssuesEventCorrectly()
    {
        Action<RedisChannel, RedisValue>? subFn = null;
        _subscriberMock
            .Setup(
                s =>
                    s.Subscribe(
                        It.IsAny<RedisChannel>(),
                        It.IsAny<Action<RedisChannel, RedisValue>>(),
                        It.IsAny<CommandFlags>()
                    )
            )
            .Callback<RedisChannel, Action<RedisChannel, RedisValue>, CommandFlags>(
                (_, handler, _) =>
                {
                    subFn = handler;
                }
            );

        INotificationReceiver<int> receiver = _interfaceFactory.GetNewReceiver<int>(
            "redis.host.invalid|SomeChannel"
        );
        receiver.StartListening();

        Assert.IsTrue(((RedisNotificationReceiver<int>)receiver).IsStarted);

        // Verify subscription has happened
        _subscriberMock.Verify(
            s =>
                s.Subscribe(
                    It.Is<RedisChannel>(c => c == "SomeChannel"),
                    It.IsAny<Action<RedisChannel, RedisValue>>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Once
        );

        Assert.IsNotNull(subFn);

        // Setup for value receive
        var receivedValues = new List<int>();
        receiver.NotificationReceived += v => receivedValues.Add(v);

        // Pretend that a value has come in
        subFn(new RedisChannel("SomeChannel", RedisChannel.PatternMode.Auto), new RedisValue("42"));

        CollectionAssert.AreEqual(new[] { 42 }, receivedValues.ToArray());
    }

    [TestMethod]
    public void ReceiverIsNotSubscribingTwice()
    {
        INotificationReceiver<int> receiver = _interfaceFactory.GetNewReceiver<int>(
            "redis.host.invalid|SomeChannel"
        );
        receiver.StartListening();
        receiver.StartListening();

        _subscriberMock.Verify(
            s =>
                s.Subscribe(
                    It.Is<RedisChannel>(c => c == "SomeChannel"),
                    It.IsAny<Action<RedisChannel, RedisValue>>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Once
        );
    }

    [TestMethod]
    public void ReceiverUnsubscribesCorrectly()
    {
        INotificationReceiver<int> receiver = _interfaceFactory.GetNewReceiver<int>(
            "redis.host.invalid|SomeChannel"
        );
        receiver.StartListening();

        receiver.StopListening();
        Assert.IsFalse(((RedisNotificationReceiver<int>)receiver).IsStarted);
        _subscriberMock.Verify(
            s =>
                s.Unsubscribe(
                    It.Is<RedisChannel>(c => c == "SomeChannel"),
                    It.IsAny<Action<RedisChannel, RedisValue>?>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Once
        );
    }

    [TestMethod]
    public void ReceiverUnsubscribesOnlyWhenConnected()
    {
        INotificationReceiver<int> receiver = _interfaceFactory.GetNewReceiver<int>(
            "redis.host.invalid|SomeChannel"
        );
        Assert.IsFalse(((RedisNotificationReceiver<int>)receiver).IsStarted);

        receiver.StopListening();
        _subscriberMock.Verify(
            s =>
                s.Subscribe(
                    It.Is<RedisChannel>(c => c == "SomeChannel"),
                    It.IsAny<Action<RedisChannel, RedisValue>>(),
                    It.IsAny<CommandFlags>()
                ),
            Times.Never
        );
    }

    [TestMethod]
    public void SenderIsCreatedCorrectly()
    {
        INotificationSender sender = _interfaceFactory.GetNewSender(
            "redis.host.invalid|SomeChannel"
        );
        Assert.IsInstanceOfType(sender, typeof(RedisNotificationSender));
        Assert.AreEqual("SomeChannel", ((RedisNotificationSender)sender).Channel);
    }

    [TestMethod]
    public void SenderIsCalledCorrectly()
    {
        INotificationSender sender = _interfaceFactory.GetNewSender(
            "redis.host.invalid|SomeChannel"
        );
        sender.Send(new { Name = "Test", Value = 42 });

        _subscriberMock.Verify(
            s =>
                s.Publish(
                    It.Is<RedisChannel>(c => c == "SomeChannel"),
                    It.Is<RedisValue>(v => v == "{\"Name\":\"Test\",\"Value\":42}"),
                    It.IsAny<CommandFlags>()
                ),
            Times.Once
        );
    }
}
