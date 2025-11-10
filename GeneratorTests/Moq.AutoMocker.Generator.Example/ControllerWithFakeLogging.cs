using Microsoft.Extensions.Logging;

namespace Moq.AutoMock.Generator.Example;

public class ControllerWithFakeLogging
{
    public Microsoft.Extensions.Logging.ILogger<ControllerWithFakeLogging> Logger { get; }
    public Microsoft.Extensions.Logging.ILoggerFactory? LoggerFactory { get; }

    public ControllerWithFakeLogging(Microsoft.Extensions.Logging.ILogger<ControllerWithFakeLogging> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public ControllerWithFakeLogging(Microsoft.Extensions.Logging.ILogger<ControllerWithFakeLogging> logger, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public void DoWork()
    {
        Logger.LogInformation("Starting work");
        Logger.LogDebug("Debug message");
        Logger.LogWarning("Warning message");
    }
}
