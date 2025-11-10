using Microsoft.Extensions.Logging.Testing;

namespace Moq.AutoMock.Generator.Example.TUnit;

public partial class ControllerWithFakeLoggingTests
{
    [Test]
    public async Task CreateInstance_WithFakeLogging_CreatesController()
    {
        AutoMocker mocker = new();
        
        mocker.AddFakeLogging();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>();

        await Assert.That(controller).IsNotNull();
        await Assert.That(controller.Logger).IsNotNull();
    }

    [Test]
    public async Task CreateInstance_WithFakeLogging_LogsMessages()
    {
        AutoMocker mocker = new();
        
        mocker.AddFakeLogging();
        var provider = mocker.Get<FakeLoggerProvider>();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>();
        controller.DoWork();

        var logs = provider.Collector.GetSnapshot();
        await Assert.That(logs.Count).IsGreaterThan(0);
        await Assert.That(logs.Any(log => log.Message == "Starting work")).IsTrue();
        await Assert.That(logs.Any(log => log.Message == "Debug message")).IsTrue();
        await Assert.That(logs.Any(log => log.Message == "Warning message")).IsTrue();
    }

    [Test]
    public async Task CreateInstance_WithFakeLogging_CreatesLoggerFactory()
    {
        AutoMocker mocker = new();
        
        mocker.AddFakeLogging();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>(enablePrivate: true);

        await Assert.That(controller.LoggerFactory).IsNotNull();
    }
}
