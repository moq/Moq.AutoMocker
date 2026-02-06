using Microsoft.Extensions.Logging.Testing;
using NUnit.Framework;

namespace Moq.AutoMock.Generator.Example.NUnit;

public class ControllerWithFakeLoggingTests
{
    [Test]
    public void CreateInstance_WithFakeLogging_CreatesController()
    {
        AutoMocker mocker = new();
        
        mocker.WithFakeLogging();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>();

        Assert.That(controller, Is.Not.Null);
        Assert.That(controller.Logger, Is.Not.Null);
    }

    [Test]
    public void CreateInstance_WithFakeLogging_LogsMessages()
    {
        AutoMocker mocker = new();
        
        mocker.WithFakeLogging();
        var provider = mocker.Get<FakeLoggerProvider>();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>();
        controller.DoWork();

        var logs = provider.Collector.GetSnapshot();
        Assert.That(logs, Is.Not.Empty);
        Assert.That(logs.Any(log => log.Message == "Starting work"), Is.True);
        Assert.That(logs.Any(log => log.Message == "Debug message"), Is.True);
        Assert.That(logs.Any(log => log.Message == "Warning message"), Is.True);
    }

    [Test]
    public void CreateInstance_WithFakeLogging_CreatesLoggerFactory()
    {
        AutoMocker mocker = new();
        
        mocker.WithFakeLogging();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>(enablePrivate: true);

        Assert.That(controller.LoggerFactory, Is.Not.Null);
    }
}
