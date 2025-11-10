using Microsoft.Extensions.Logging.Testing;

namespace Moq.AutoMock.Generator.Example.MSUnit;

[TestClass]
public class ControllerWithFakeLoggingTests
{
    [TestMethod]
    public void CreateInstance_WithFakeLogging_CreatesController()
    {
        AutoMocker mocker = new();
        
        mocker.AddFakeLogging();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>();

        Assert.IsNotNull(controller);
        Assert.IsNotNull(controller.Logger);
    }

    [TestMethod]
    public void CreateInstance_WithFakeLogging_LogsMessages()
    {
        AutoMocker mocker = new();
        
        mocker.AddFakeLogging();
        var provider = mocker.Get<FakeLoggerProvider>();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>();
        controller.DoWork();

        var logs = provider.Collector.GetSnapshot();
        Assert.IsNotNull(logs);
        CollectionAssert.AllItemsAreNotNull(logs.ToList());
        Assert.IsTrue(logs.Any(log => log.Message == "Starting work"));
        Assert.IsTrue(logs.Any(log => log.Message == "Debug message"));
        Assert.IsTrue(logs.Any(log => log.Message == "Warning message"));
    }

    [TestMethod]
    public void CreateInstance_WithFakeLogging_CreatesLoggerFactory()
    {
        AutoMocker mocker = new();
        
        mocker.AddFakeLogging();

        ControllerWithFakeLogging controller = mocker.CreateInstance<ControllerWithFakeLogging>(enablePrivate: true);

        Assert.IsNotNull(controller.LoggerFactory);
    }
}
