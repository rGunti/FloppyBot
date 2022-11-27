using FloppyBot.Base.TextFormatting;
using FloppyBot.Commands.Core.Entities;
using FloppyBot.Commands.Custom.Execution.Administration;
using FloppyBot.Commands.Custom.Storage;
using FloppyBot.Commands.Custom.Storage.Entities;
using Moq;

namespace FloppyBot.Commands.Custom.Tests.Execution;

[TestClass]
public class CustomCommandAdministrationCommandsTests
{
    private readonly Mock<ICounterStorageService> _counterStorageServiceMock;
    private readonly Mock<ICustomCommandService> _customCommandServiceMock;
    private readonly CustomCommandAdministrationCommands _host;

    public CustomCommandAdministrationCommandsTests()
    {
        _customCommandServiceMock = new Mock<ICustomCommandService>();
        _counterStorageServiceMock = new Mock<ICounterStorageService>();
        _host = new CustomCommandAdministrationCommands(
            _customCommandServiceMock.Object,
            _counterStorageServiceMock.Object);
    }

    [TestMethod]
    public void CreateCommand()
    {
        _customCommandServiceMock
            .Setup(s => s.CreateSimpleCommand(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string, string>((channelId, commandName, response) => true);

        CommandResult result = _host.CreateCommand("Mock/UnitTest", "mycommand", "This is my cool command");

        Assert.AreEqual(
            new CommandResult(
                CommandOutcome.Success,
                CustomCommandAdministrationCommands.REPLY_CREATE_SUCCESS.Format(new
                {
                    CommandName = "mycommand"
                })),
            result);
        _customCommandServiceMock
            .Verify(s => s.CreateSimpleCommand(
                    It.Is<string>(c => c == "Mock/UnitTest"),
                    It.Is<string>(c => c == "mycommand"),
                    It.Is<string>(c => c == "This is my cool command")),
                Times.Once);
    }

    [TestMethod]
    public void CreateExisting()
    {
        _customCommandServiceMock
            .Setup(s => s.CreateSimpleCommand(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string, string>((channelId, commandName, response) => false);

        CommandResult result = _host.CreateCommand("Mock/UnitTest", "mycommand", "This is my cool command");

        Assert.AreEqual(
            new CommandResult(
                CommandOutcome.Failed,
                CustomCommandAdministrationCommands.REPLY_CREATE_FAILED.Format(new
                {
                    CommandName = "mycommand"
                })),
            result);
        _customCommandServiceMock
            .Verify(s => s.CreateSimpleCommand(
                    It.Is<string>(c => c == "Mock/UnitTest"),
                    It.Is<string>(c => c == "mycommand"),
                    It.Is<string>(c => c == "This is my cool command")),
                Times.Once);
    }

    [TestMethod]
    public void DeleteCommand()
    {
        _customCommandServiceMock
            .Setup(s => s.DeleteCommand(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((channelId, commandName) => true);

        CommandResult result = _host.DeleteCommand("Mock/UnitTest", "mycommand");
        Assert.AreEqual(
            new CommandResult(
                CommandOutcome.Success,
                CustomCommandAdministrationCommands.REPLY_DELETE_SUCCESS.Format(new
                {
                    CommandName = "mycommand"
                })),
            result);
    }

    [TestMethod]
    public void DeleteFailed()
    {
        _customCommandServiceMock
            .Setup(s => s.DeleteCommand(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((channelId, commandName) => false);

        CommandResult result = _host.DeleteCommand("Mock/UnitTest", "mycommand");
        Assert.AreEqual(
            new CommandResult(
                CommandOutcome.Failed,
                CustomCommandAdministrationCommands.REPLY_DELETE_FAILED.Format(new
                {
                    CommandName = "mycommand"
                })),
            result);
    }

    [TestMethod]
    public void SetCounterAbsolute()
    {
        _customCommandServiceMock
            .Setup(s => s.GetCommand(
                It.Is<string>(c => c == "Mock/UnitTest"),
                It.Is<string>(c => c == "mycommand")))
            .Returns<string, string>((_, _) => new CustomCommandDescription
            {
                Id = "abc123"
            });
        _counterStorageServiceMock
            .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<int>()))
            .Verifiable();
        _counterStorageServiceMock
            .Setup(s => s.Peek(
                It.Is<string>(c => c == "abc123")))
            .Returns<string>(_ => 5);

        CommandResult result = _host.SetCounter("Mock/UnitTest", "mycommand", "5");
        Assert.AreEqual(
            new CommandResult(
                CommandOutcome.Success,
                CustomCommandAdministrationCommands.REPLY_COUNTER_SET.Format(new
                {
                    CommandName = "mycommand",
                    Counter = 5
                })),
            result);
        _counterStorageServiceMock
            .Verify(s => s.Set(
                    It.Is<string>(c => c == "abc123"),
                    It.Is<int>(c => c == 5)),
                Times.Once);
    }

    [DataTestMethod]
    [DataRow("+5", 5)]
    [DataRow("-10", -10)]
    public void SetCounterRelative(string input, int expectedIncrement)
    {
        _customCommandServiceMock
            .Setup(s => s.GetCommand(
                It.Is<string>(c => c == "Mock/UnitTest"),
                It.Is<string>(c => c == "mycommand")))
            .Returns<string, string>((_, _) => new CustomCommandDescription
            {
                Id = "abc123"
            });
        _counterStorageServiceMock
            .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<int>()))
            .Verifiable();
        _counterStorageServiceMock
            .Setup(s => s.Increase(It.IsAny<string>(), It.IsAny<int>()))
            .Returns<string, int>((_, increment) => 10 + increment);

        CommandResult result = _host.SetCounter("Mock/UnitTest", "mycommand", input);
        Assert.AreEqual(
            new CommandResult(CommandOutcome.Success, CustomCommandAdministrationCommands.REPLY_COUNTER_SET.Format(new
            {
                CommandName = "mycommand",
                Counter = 10 + expectedIncrement
            })),
            result);

        _counterStorageServiceMock
            .Verify(s => s.Increase(
                    It.Is<string>(c => c == "abc123"),
                    It.Is<int>(i => i == expectedIncrement)),
                Times.Once);
        _counterStorageServiceMock
            .Verify(s => s.Set(
                    It.Is<string>(c => c == "abc123"),
                    It.IsAny<int>()),
                Times.Never);
    }

    [TestMethod]
    public void SetCounterClear()
    {
        _customCommandServiceMock
            .Setup(s => s.GetCommand(
                It.Is<string>(c => c == "Mock/UnitTest"),
                It.Is<string>(c => c == "mycommand")))
            .Returns<string, string>((_, _) => new CustomCommandDescription
            {
                Id = "abc123"
            });
        _counterStorageServiceMock
            .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<int>()))
            .Verifiable();

        CommandResult result = _host.SetCounter("Mock/UnitTest", "mycommand", "clear");
        Assert.AreEqual(
            new CommandResult(
                CommandOutcome.Success,
                CustomCommandAdministrationCommands.REPLY_COUNTER_SET.Format(new
                {
                    CommandName = "mycommand",
                    Counter = 0
                })),
            result);
        _counterStorageServiceMock
            .Verify(s => s.Set(
                    It.Is<string>(c => c == "abc123"),
                    It.Is<int>(c => c == 0)),
                Times.Once);
    }

    [TestMethod]
    public void SetUnknownCommand()
    {
        _customCommandServiceMock
            .Setup(s => s.GetCommand(
                It.Is<string>(c => c == "Mock/UnitTest"),
                It.Is<string>(c => c == "mycommand")))
            .Returns<string, string>((_, _) => null);

        CommandResult result = _host.SetCounter("Mock/UnitTest", "mycommand", "clear");
        Assert.AreEqual(
            new CommandResult(
                CommandOutcome.Failed,
                CustomCommandAdministrationCommands.REPLY_COMMAND_NOT_FOUND.Format(new
                {
                    CommandName = "mycommand"
                })),
            result);
        _counterStorageServiceMock
            .Verify(s => s.Set(
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never);
    }

    [TestMethod]
    public void SetCommandFailed()
    {
        _customCommandServiceMock
            .Setup(s => s.GetCommand(
                It.Is<string>(c => c == "Mock/UnitTest"),
                It.Is<string>(c => c == "mycommand")))
            .Returns<string, string>((_, _) => new CustomCommandDescription
            {
                Id = "abc123"
            });

        CommandResult result = _host.SetCounter("Mock/UnitTest", "mycommand", "notAThing");
        Assert.AreEqual(
            new CommandResult(
                CommandOutcome.Failed,
                CustomCommandAdministrationCommands.REPLY_COUNTER_PARAMS_UNKNOWN),
            result);
        _counterStorageServiceMock
            .Verify(s => s.Set(
                    It.IsAny<string>(),
                    It.IsAny<int>()),
                Times.Never);
    }
}
