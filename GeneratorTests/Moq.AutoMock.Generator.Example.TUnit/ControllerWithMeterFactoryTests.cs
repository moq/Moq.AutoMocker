using System.Diagnostics.Metrics;

namespace Moq.AutoMock.Generator.Example.TUnit;

public partial class ControllerWithMeterFactoryTests
{
    [Test]
    public async Task CreateInstance_WithMeterFactory_CreatesController()
    {
        AutoMocker mocker = new();

        mocker.WithMeterFactory();

        ControllerWithMeterFactory controller = mocker.CreateInstance<ControllerWithMeterFactory>();

        await Assert.That(controller).IsNotNull();
        await Assert.That(controller.MeterFactory).IsNotNull();
    }

    [Test]
    public async Task CreateInstance_WithMeterFactory_RecordsMetrics()
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

        await Assert.That(measurements).Count().IsEqualTo(1);
        await Assert.That(measurements[0]).IsEqualTo(1L);
    }
}
