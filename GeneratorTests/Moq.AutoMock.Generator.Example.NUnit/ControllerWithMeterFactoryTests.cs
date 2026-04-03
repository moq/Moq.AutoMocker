using System.Diagnostics.Metrics;
using NUnit.Framework;

namespace Moq.AutoMock.Generator.Example.NUnit;
public class ControllerWithMeterFactoryTests
{
    [Test]
    public void CreateInstance_WithMeterFactory_CreatesController()
    {
        AutoMocker mocker = new();

        mocker.WithMeterFactory();

        ControllerWithMeterFactory controller = mocker.CreateInstance<ControllerWithMeterFactory>();

        Assert.That(controller, Is.Not.Null);
        Assert.That(controller.MeterFactory, Is.Not.Null);
    }

    [Test]
    public void CreateInstance_WithMeterFactory_RecordsMetrics()
    {
        AutoMocker mocker = new();

        mocker.WithMeterFactory();

        var measurements = new List<long>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Name == "requests")
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) 
            => measurements.Add(measurement));
        listener.Start();

        ControllerWithMeterFactory controller = mocker.CreateInstance<ControllerWithMeterFactory>();
        controller.HandleRequest();

        Assert.That(measurements, Has.Count.EqualTo(1));
        Assert.That(measurements[0], Is.EqualTo(1L));
    }
}
