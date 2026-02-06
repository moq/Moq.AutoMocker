using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace Moq.AutoMock.Generator.Example.xUnit3;

public class ControllerWithFakeLoggingTests
{
    [Fact]
    public void CreateInstance_WithFakeLogging_CreatesController()
    {
        AutoMocker mocker = new();
        
        mocker.WithFakeLogging();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>();

        Assert.NotNull(controller);
        Assert.NotNull(controller.Logger);
    }

    [Fact]
    public void CreateInstance_WithFakeLogging_LogsMessages()
    {
        AutoMocker mocker = new();
        
        mocker.WithFakeLogging();
        var provider = mocker.Get<FakeLoggerProvider>();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>();
        controller.DoWork();

        var logs = provider.Collector.GetSnapshot();
        Assert.NotEmpty(logs);
        Assert.Contains(logs, log => log.Message == "Starting work");
        Assert.Contains(logs, log => log.Message == "Debug message");
        Assert.Contains(logs, log => log.Message == "Warning message");
    }

    [Fact]
    public void CreateInstance_WithFakeLogging_CreatesLoggerFactory()
    {
        AutoMocker mocker = new();
        
        mocker.WithFakeLogging();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>(enablePrivate: true);

        Assert.NotNull(controller.LoggerFactory);
    }
}
