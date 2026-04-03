# Meter Factory Extension Generator

When your test project references `System.Diagnostics.DiagnosticSource` (version 10.0.0 or later), this generator creates a `WithMeterFactory()` extension method for `AutoMocker` that provides a real `IMeterFactory` implementation for metrics testing.

## Features

- Automatically generates when `System.Diagnostics.DiagnosticSource` 10.0+ is referenced
- Provides a real `IMeterFactory` that creates actual `Meter` instances
- Forwards `Create(MeterOptions)` directly to `new Meter(options)`
- Tracks created meters and disposes them on cleanup

## Usage

```csharp
using System.Diagnostics.Metrics;

public class MetricsService
{
    private readonly Meter _meter;
    private readonly Counter<long> _requestCounter;

    public MetricsService(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(new MeterOptions("MyApp"));
        _requestCounter = _meter.CreateCounter<long>("requests");
    }

    public void HandleRequest()
    {
        _requestCounter.Add(1);
    }
}

[TestClass]
public class MetricsServiceTests
{
    [TestMethod]
    public void Test_WithMeterFactory()
    {
        // Arrange
        AutoMocker mocker = new();
        mocker.WithMeterFactory();

        // Act
        var service = mocker.CreateInstance<MetricsService>();
        service.HandleRequest();

        // Assert — service was constructed without exceptions
        Assert.IsNotNull(service);
    }
}
```

## Generated Extension Method

The generator creates:

```csharp
public static AutoMocker WithMeterFactory(this AutoMocker mocker)
{
    // Registers a MeterFactoryResolver that provides IMeterFactory
    // Returns mocker for fluent chaining
}
```

## How It Works

The `WithMeterFactory()` method:

1. Creates a `MeterFactoryResolver` instance
2. Inserts the resolver after the `CacheResolver` in the resolver chain
3. When `IMeterFactory` is requested, the resolver returns an `AutoMockerMeterFactory`
4. The `AutoMockerMeterFactory.Create(MeterOptions)` call forwards to `new Meter(options)`
5. Created meters are tracked and disposed when the factory is disposed

This ensures any class depending on `IMeterFactory` receives a working implementation that creates real `Meter` instances, without requiring a full dependency injection container.

## Advanced Usage

### Collecting Metrics with MeterListener

```csharp
[TestMethod]
public void Test_MetricsAreRecorded()
{
    // Arrange
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
    listener.SetMeasurementEventCallback<long>((instrument, measurement, tags, state) =>
    {
        measurements.Add(measurement);
    });
    listener.Start();

    // Act
    var service = mocker.CreateInstance<MetricsService>();
    service.HandleRequest();

    // Assert
    listener.RecordObservableInstruments();
    Assert.AreEqual(1, measurements.Count);
    Assert.AreEqual(1L, measurements[0]);
}
```

### Combining with Other Extensions

```csharp
[TestMethod]
public void Test_CombinedExtensions()
{
    AutoMocker mocker = new();
    mocker.WithMeterFactory()
          .WithFakeLogging();

    var service = mocker.CreateInstance<InstrumentedService>();
    service.Process();

    // Verify both metrics and logging behavior
}
```

## Disabling the Generator

You can disable this generator using an MSBuild property in your test project's `.csproj` file:

```xml
<PropertyGroup>
  <EnableMoqAutoMockerMeterFactoryGenerator>false</EnableMoqAutoMockerMeterFactoryGenerator>
</PropertyGroup>
```

### Example: Disabling in Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    
    <!-- Disable Meter Factory Extension Generator -->
    <EnableMoqAutoMockerMeterFactoryGenerator>false</EnableMoqAutoMockerMeterFactoryGenerator>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq.AutoMock" Version="4.0.1" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Extension Method Not Available

1. Verify your project targets .NET 10.0 or later (or references `System.Diagnostics.DiagnosticSource` 10.0+)
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you're using the `Moq.AutoMock` namespace

### IMeterFactory Not Resolving

1. Make sure you call `WithMeterFactory()` before calling `CreateInstance<T>()`
2. Verify the service under test accepts `IMeterFactory` (not a concrete type)

## Best Practices

- Call `WithMeterFactory()` early in your test setup, before creating instances
- Use `MeterListener` to capture and assert on metric measurements
- Combine with `WithFakeLogging()` for comprehensive observability testing
- The factory creates real `Meter` instances, so instruments work as expected

## See Also

- [AutoMocker API Reference](../Moq.AutoMock/AutoMocker.md)
- [Source Generators Overview](../SourceGenerators.md)
- [Moq.AutoMock on NuGet](https://www.nuget.org/packages/Moq.AutoMock)
