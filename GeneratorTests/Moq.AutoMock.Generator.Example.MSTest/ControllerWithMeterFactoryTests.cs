using System.Diagnostics.Metrics;

namespace Moq.AutoMock.Generator.Example.MSTest;

[TestClass]
public class ControllerWithMeterFactoryTests
{
    [TestMethod]
    public void CreateInstance_WithMeterFactory_CreatesController()
    {
        AutoMocker mocker = new();

        mocker.WithMeterFactory();

        ControllerWithMeterFactory controller = mocker.CreateInstance<ControllerWithMeterFactory>();

        Assert.IsNotNull(controller);
        Assert.IsNotNull(controller.MeterFactory);
    }

    [TestMethod]
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

        Assert.HasCount(1, measurements);
        Assert.AreEqual(1L, measurements[0]);
    }
}
