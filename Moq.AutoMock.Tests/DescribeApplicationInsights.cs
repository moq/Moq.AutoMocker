using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using OpenTelemetry.Metrics;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeApplicationInsights
{
    [TestMethod]
    public void WithApplicationInsights_ProvidesActivityList()
    {
        var mocker = new AutoMocker();
        mocker.WithApplicationInsights();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackRequestTelemetry("TestRequest1");
        service.TrackRequestTelemetry("TestRequest2");


        var activities = mocker.GetApplicationInsightsActivities();
        Assert.HasCount(2, activities);
        Assert.AreEqual("TestRequest1", activities[0].OperationName);
        Assert.AreEqual("TestRequest2", activities[1].OperationName);
    }

    [TestMethod]
    public void WithApplicationInsights_AllowsTrackingEvents()
    {
        var mocker = new AutoMocker();
        mocker.WithApplicationInsights();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackEvent("UserLoggedIn");
        service.TrackEvent("UserLoggedOut");

        var logRecords = mocker.GetApplicationInsightsLogRecords();
        Assert.HasCount(2, logRecords);
        Assert.AreEqual("UserLoggedIn", logRecords[0].Attributes?[0].Value);
        Assert.AreEqual("UserLoggedOut", logRecords[1].Attributes?[0].Value);
    }

    [TestMethod]
    public void WithApplicationInsights_AllowsTrackingMetrics()
    {
        var mocker = new AutoMocker();
        mocker.WithApplicationInsights();

        var service = mocker.CreateInstance<ServiceWithApplicationInsights>();

        service.TrackMetric("ResponseTime", 123.45);
        service.TrackMetric("ItemsCount", 42);

        //Flush the metrics
        mocker.Get<TelemetryClient>().Flush();
        var metrics = mocker.GetApplicationInsightsMetrics().Select(x => new MetricSnapshot(x)).ToList();
        Assert.HasCount(2, metrics);
        Assert.AreEqual(123.45, metrics[0].MetricPoints[^1].GetHistogramSum());
        Assert.AreEqual(42, metrics[1].MetricPoints[^1].GetHistogramSum());
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

        public void TrackRequestTelemetry(string name)
        {
            using var operation = telemetryClient.StartOperation<RequestTelemetry>(name);
        }
    }
}
