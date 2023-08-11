using System;
using System.Collections.Generic;
using FakeItEasy;
using FloppyBot.Base.Testing;
using FloppyBot.Communication.Redis.Config;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace FloppyBot.Communication.Redis.Tests;

[TestClass]
public class RedisNotificationInterfaceTests
{
    private readonly RedisNotificationInterfaceFactory _interfaceFactory;
    private readonly ISubscriber _subscriber;

    public RedisNotificationInterfaceTests()
    {
        _subscriber = A.Fake<ISubscriber>();

        var connectionMultiplexer = A.Fake<IConnectionMultiplexer>();
        A.CallTo(() => connectionMultiplexer.GetSubscriber(An<object?>.Ignored))
            .Returns(_subscriber);

        var factory = A.Fake<IRedisConnectionFactory>();
        A.CallTo(() => factory.GetMultiplexer(An<RedisConnectionConfig>.Ignored))
            .Returns(connectionMultiplexer);

        _interfaceFactory = new RedisNotificationInterfaceFactory(
            LoggingUtils.GetLogger<RedisNotificationInterfaceFactory>(),
            factory
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

        A.CallTo(
                () =>
                    _subscriber.Subscribe(
                        A<RedisChannel>.That.IsEqualTo("SomeChannel"),
                        A<Action<RedisChannel, RedisValue>>.Ignored,
                        A<CommandFlags>.Ignored
                    )
            )
            .Invokes(
                (
                    RedisChannel channel,
                    Action<RedisChannel, RedisValue> handler,
                    CommandFlags flags
                ) =>
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
        A.CallTo(
                () =>
                    _subscriber.Subscribe(
                        An<RedisChannel>.That.IsEqualTo("SomeChannel"),
                        An<Action<RedisChannel, RedisValue>>.Ignored,
                        An<CommandFlags>.Ignored
                    )
            )
            .MustHaveHappenedOnceExactly();

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

        A.CallTo(
                () =>
                    _subscriber.Subscribe(
                        A<RedisChannel>.That.IsEqualTo("SomeChannel"),
                        A<Action<RedisChannel, RedisValue>>.Ignored,
                        A<CommandFlags>.Ignored
                    )
            )
            .MustHaveHappenedOnceExactly();
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
        A.CallTo(
                () =>
                    _subscriber.Subscribe(
                        A<RedisChannel>.That.IsEqualTo("SomeChannel"),
                        A<Action<RedisChannel, RedisValue>>.Ignored,
                        A<CommandFlags>.Ignored
                    )
            )
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void ReceiverUnsubscribesOnlyWhenConnected()
    {
        INotificationReceiver<int> receiver = _interfaceFactory.GetNewReceiver<int>(
            "redis.host.invalid|SomeChannel"
        );
        Assert.IsFalse(((RedisNotificationReceiver<int>)receiver).IsStarted);

        receiver.StopListening();
        A.CallTo(
                () =>
                    _subscriber.Subscribe(
                        A<RedisChannel>.That.IsEqualTo("SomeChannel"),
                        A<Action<RedisChannel, RedisValue>>.Ignored,
                        A<CommandFlags>.Ignored
                    )
            )
            .MustNotHaveHappened();
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

        A.CallTo(
                () =>
                    _subscriber.Publish(
                        An<RedisChannel>.That.IsEqualTo("SomeChannel"),
                        An<RedisValue>.That.IsEqualTo("{\"Name\":\"Test\",\"Value\":42}"),
                        An<CommandFlags>.Ignored
                    )
            )
            .MustHaveHappenedOnceExactly();
    }
}
