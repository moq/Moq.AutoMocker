using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Moq.AutoMock.Tests;

public class LogProcessor : BaseProcessor<LogRecord>
{
    private readonly List<LogRecord> _records = [];
    public IReadOnlyList<LogRecord> SentItems => _records.AsReadOnly();
    public override void OnEnd(LogRecord logRecord)
    {
        //TODO: Threading
        _records.Add(logRecord);
    }
}

public class MyMetricReader : MetricReader
{
    protected override bool OnCollect(int timeoutMilliseconds)
    {
        return base.OnCollect(timeoutMilliseconds);
    }
}

static partial class AutoMockerApplicationInsightsExtensions2
{
    /// <summary>
    /// This method sets up <see cref="AutoMocker"/> with Application Insights services,
    /// ensuring TelemetryClient is properly mocked and doesn't make real API calls.
    /// This allows telemetry tracking calls to be verified in tests.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker"/> instance.</param>
    /// <returns>The same <see cref="AutoMocker"/> instance passed as parameter, allowing chained calls.</returns>
    public static AutoMocker WithApplicationInsights2(this AutoMocker mocker)
    {
        if (mocker == null)
        {
            throw new ArgumentNullException(nameof(mocker));
        }

        // Create TelemetryConfiguration with the fake channel
        var telemetryConfiguration = new TelemetryConfiguration
        {
            SamplingRatio = 1.0f,
            ConnectionString = "InstrumentationKey=" + Guid.Empty
        };

        var logItems = new List<LogRecord>();
        var metricItems = new List<OpenTelemetry.Metrics.Metric>();
        var activityItems = new List<Activity>();
        telemetryConfiguration.ConfigureOpenTelemetryBuilder(b => b
            .WithLogging(l => l.AddInMemoryExporter(logItems))
            .WithMetrics(m => m.AddInMemoryExporter(metricItems))
            .WithTracing(t => t.AddInMemoryExporter(activityItems)));


        // Create TelemetryClient with the configuration
        var telemetryClient = new TelemetryClient(telemetryConfiguration);

        // Register the instances
        //TODO: Interface
        mocker.Use(telemetryConfiguration);
        mocker.Use(telemetryClient);

        return mocker;
    }

    /// <summary>
    /// Gets the collection of telemetry items that have been sent through the Application Insights TelemetryClient.
    /// This method retrieves telemetry from the <see cref="FakeTelemetryChannel"/> that was configured by <see cref="WithApplicationInsights"/>.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker"/> instance.</param>
    /// <returns>A read-only list of telemetry items that have been sent.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mocker"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when Application Insights has not been configured using <see cref="WithApplicationInsights"/>.</exception>
    public static IReadOnlyList<LogRecord> GetSentTelemetry2(this AutoMocker mocker)
    {
        if (mocker == null)
        {
            throw new ArgumentNullException(nameof(mocker));
        }

        var processor = mocker.Get<LogProcessor>();
        if (processor == null)
        {
            throw new InvalidOperationException("Application Insights has not been configured. Call WithApplicationInsights() first.");
        }

        return processor.SentItems;
    }
}

[TestClass]
public class DescribeApplicationInsights
{
    [TestMethod]
    public void WithApplicationInsights_AllowsVerificationOfTrackedEvents()
    {
        var mocker = new AutoMocker();
        mocker.WithApplicationInsights2();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackEvent("UserLoggedIn");
        service.TrackEvent("UserLoggedOut");

        // Verify telemetry was tracked
        var telemetryEvents = mocker.GetSentTelemetry2();

        Assert.HasCount(2, telemetryEvents);
        var eventTelemetry1 = Assert.IsInstanceOfType<EventTelemetry>(telemetryEvents[0]);
        Assert.AreEqual("UserLoggedIn", eventTelemetry1.Name);
        var eventTelemetry2 = Assert.IsInstanceOfType<EventTelemetry>(telemetryEvents[1]);
        Assert.AreEqual("UserLoggedOut", eventTelemetry2.Name);
    }

    [TestMethod]
    public void WithApplicationInsights_AllowsVerificationOfTrackedMetrics()
    {
        var mocker = new AutoMocker();
        mocker.WithApplicationInsights2();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackMetric("ResponseTime", 123.45);
        service.TrackMetric("ItemsCount", 42);


        // Verify telemetry was tracked

        var telemetryEvents = mocker.GetSentTelemetry2();
        Assert.HasCount(2, telemetryEvents);
        
        
        var metricTelemetry1 = Assert.IsInstanceOfType<MetricTelemetry>(telemetryEvents[0]);
        Assert.AreEqual("ResponseTime", metricTelemetry1.Name);
        Assert.AreEqual(123.45, metricTelemetry1.Value);
        var metricTelemetry2 = Assert.IsInstanceOfType<MetricTelemetry>(telemetryEvents[1]);
        Assert.AreEqual("ItemsCount", metricTelemetry2.Name);
        Assert.AreEqual(42, metricTelemetry2.Value);
    }

    [TestMethod]
    public void GetSentTelemetry_ReturnsAllSentTelemetryItems()
    {
        var mocker = new AutoMocker();
        mocker.WithApplicationInsights2();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackEvent("Event1");
        service.TrackMetric("Metric1", 42.0);
        service.TrackEvent("Event2");

        var sentTelemetry = mocker.GetSentTelemetry2();

        Assert.HasCount(3, sentTelemetry);
        Assert.IsInstanceOfType<EventTelemetry>(sentTelemetry[0]);
        Assert.IsInstanceOfType<MetricTelemetry>(sentTelemetry[1]);
        Assert.IsInstanceOfType<EventTelemetry>(sentTelemetry[2]);
    }

    private class ServiceWithApplicationInsights(TelemetryClient telemetryClient)
    {
        public void TrackEvent(string @event)
        {
            telemetryClient.TrackEvent(@event);
        }

        public void TrackMetric(string name, double value)
        {
            telemetryClient.TrackMetric(name, value);
        }
    }
}
