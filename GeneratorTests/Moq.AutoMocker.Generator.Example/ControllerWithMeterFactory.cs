using System.Diagnostics.Metrics;

namespace Moq.AutoMock.Generator.Example;
public class ControllerWithMeterFactory
{
    public IMeterFactory MeterFactory { get; }
    private readonly Counter<long> _requestCounter;

    public ControllerWithMeterFactory(IMeterFactory meterFactory)
    {
        MeterFactory = meterFactory ?? throw new ArgumentNullException(nameof(meterFactory));
        var meter = meterFactory.Create(new MeterOptions("TestApp"));
        _requestCounter = meter.CreateCounter<long>("requests");
    }

    public void HandleRequest()
    {
        _requestCounter.Add(1);
    }
}
