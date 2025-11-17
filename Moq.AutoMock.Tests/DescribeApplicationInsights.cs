using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeApplicationInsights
{
    [TestMethod]
    public void AddApplicationInsights_AllowsVerificationOfTrackedEvents()
    {
        var mocker = new AutoMocker();
        mocker.AddApplicationInsights();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackEvent("UserLoggedIn");
        service.TrackEvent("UserLoggedOut");

        // Verify telemetry was tracked
        var telemetryEvents = mocker.GetSentTelemetry();

        Assert.HasCount(2, telemetryEvents);
        var eventTelemetry1 = Assert.IsInstanceOfType<EventTelemetry>(telemetryEvents[0]);
        Assert.AreEqual("UserLoggedIn", eventTelemetry1.Name);
        var eventTelemetry2 = Assert.IsInstanceOfType<EventTelemetry>(telemetryEvents[1]);
        Assert.AreEqual("UserLoggedOut", eventTelemetry2.Name);
    }

    [TestMethod]
    public void AddApplicationInsights_AllowsVerificationOfTrackedMetrics()
    {
        var mocker = new AutoMocker();
        mocker.AddApplicationInsights();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackMetric("ResponseTime", 123.45);
        service.TrackMetric("ItemsCount", 42);


        // Verify telemetry was tracked
        var telemetryEvents = mocker.GetSentTelemetry();
        Assert.HasCount(2, telemetryEvents);
        
        
        var metricTelemetry1 = Assert.IsInstanceOfType<MetricTelemetry>(telemetryEvents[0]);
        Assert.AreEqual("ResponseTime", metricTelemetry1.Name);
        Assert.AreEqual(123.45, metricTelemetry1.Sum);
        var metricTelemetry2 = Assert.IsInstanceOfType<MetricTelemetry>(telemetryEvents[1]);
        Assert.AreEqual("ItemsCount", metricTelemetry2.Name);
        Assert.AreEqual(42, metricTelemetry2.Sum);
    }

    [TestMethod]
    public void GetSentTelemetry_ReturnsAllSentTelemetryItems()
    {
        var mocker = new AutoMocker();
        mocker.AddApplicationInsights();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackEvent("Event1");
        service.TrackMetric("Metric1", 42.0);
        service.TrackEvent("Event2");

        var sentTelemetry = mocker.GetSentTelemetry();

        Assert.HasCount(3, sentTelemetry);
        Assert.IsInstanceOfType(sentTelemetry[0], typeof(EventTelemetry));
        Assert.IsInstanceOfType(sentTelemetry[1], typeof(MetricTelemetry));
        Assert.IsInstanceOfType(sentTelemetry[2], typeof(EventTelemetry));
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
