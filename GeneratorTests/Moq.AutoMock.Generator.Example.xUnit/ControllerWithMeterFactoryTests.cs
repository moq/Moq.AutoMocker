using System.Diagnostics.Metrics;
using Xunit;

namespace Moq.AutoMock.Generator.Example.xUnit;
public class ControllerWithMeterFactoryTests
{
    [Fact]
    public void CreateInstance_WithMeterFactory_CreatesController()
    {
        AutoMocker mocker = new();

        mocker.WithMeterFactory();

        ControllerWithMeterFactory controller = mocker.CreateInstance<ControllerWithMeterFactory>();

        Assert.NotNull(controller);
        Assert.NotNull(controller.MeterFactory);
    }

    [Fact]
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

        Assert.Single(measurements);
        Assert.Equal(1L, measurements[0]);
    }
}
