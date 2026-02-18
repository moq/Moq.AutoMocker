# Application Insights Extension Generator

When your test project references `Microsoft.ApplicationInsights`, this generator creates `WithApplicationInsights()` and related extension methods for `AutoMocker` that enable testing of telemetry tracking code without making real API calls.

The generator is version-aware: it produces different code depending on whether you are using Application Insights 2.x or 3.x.

## Features

- Automatically generates when `Microsoft.ApplicationInsights` is referenced
- Provides a `TelemetryClient` that doesn't make real API calls
- **Application Insights 2.x**: Uses a `FakeTelemetryChannel` to capture `ITelemetry` items
- **Application Insights 3.x**: Uses OpenTelemetry in-memory exporters to capture log records, metrics, and activities
- **Application Insights 3.x without `OpenTelemetry.Exporter.InMemory`**: Generates an `[Obsolete]` stub that throws.
- Allows verification of telemetry calls in tests

## Usage

### Application Insights 3.x (with OpenTelemetry)

Application Insights 3.x uses OpenTelemetry under the hood. To use the generated extension methods, your test project must reference both `Microsoft.ApplicationInsights` (3.x) and `OpenTelemetry.Exporter.InMemory`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.ApplicationInsights" Version="3.0.0" />
  <PackageReference Include="OpenTelemetry.Exporter.InMemory" Version="1.15.0" />
</ItemGroup>
```

#### Verifying Tracked Events (Log Records)

```csharp
[TestClass]
public class MyServiceTests
{
    [TestMethod]
    public void DoWork_TracksEvents()
    {
        AutoMocker mocker = new();
        mocker.WithApplicationInsights();

        var service = mocker.CreateInstance<MyService>();
        service.DoWork();

        var logRecords = mocker.GetApplicationInsightsLogRecords();
        Assert.HasCount(2, logRecords);
    }
}
```

#### Verifying Tracked Activities (Traces)

```csharp
[TestMethod]
public void ProcessRequest_TracksOperations()
{
    AutoMocker mocker = new();
    mocker.WithApplicationInsights();

    var service = mocker.CreateInstance<MyService>();
    service.ProcessRequest("TestRequest");

    var activities = mocker.GetApplicationInsightsActivities();
    Assert.HasCount(1, activities);
    Assert.AreEqual("TestRequest", activities[0].OperationName);
}
```

#### Verifying Tracked Metrics

```csharp
[TestMethod]
public void TrackMetrics_CapturesValues()
{
    AutoMocker mocker = new();
    mocker.WithApplicationInsights();

    var service = mocker.CreateInstance<MyService>();
    service.RecordResponseTime(123.45);

    // Flush to ensure metrics are exported
    mocker.Get<TelemetryClient>().Flush();

    var metrics = mocker.GetApplicationInsightsMetrics();
    Assert.HasCount(1, metrics);
}
```

### Application Insights 2.x

With Application Insights 2.x, the generator uses a `FakeTelemetryChannel` to intercept telemetry. No additional packages are needed beyond `Microsoft.ApplicationInsights` 2.x:

```csharp
[TestMethod]
public void DoWork_TracksEvents()
{
    AutoMocker mocker = new();
    mocker.WithApplicationInsights();

    var service = mocker.CreateInstance<MyService>();
    service.DoWork();

    // V2 uses ITelemetry items via GetSentTelemetry()
    var telemetry = mocker.GetSentTelemetry();
    Assert.HasCount(2, telemetry);
}
```

### Application Insights 3.x without OpenTelemetry.Exporter.InMemory

If your project references Application Insights 3.x but does **not** reference `OpenTelemetry.Exporter.InMemory`, the generator produces a stub method marked `[Obsolete]` that throws `InvalidOperationException` at runtime:

```csharp
// Compiler warning: "To use WithApplicationInsights with Application Insights 3.x,
// add a reference to the OpenTelemetry.Exporter.InMemory NuGet package."
mocker.WithApplicationInsights(); // Throws InvalidOperationException
```

To fix this, add `OpenTelemetry.Exporter.InMemory` to your project and rebuild.

## Generated Extension Methods

### Application Insights 3.x (with InMemory exporter)

```csharp
/// <summary>
/// Sets up AutoMocker with Application Insights 3.x using OpenTelemetry in-memory exporters.
/// </summary>
public static AutoMocker WithApplicationInsights(this AutoMocker mocker)

/// <summary>
/// Gets captured log records from the in-memory exporter.
/// </summary>
public static IList<LogRecord> GetApplicationInsightsLogRecords(this AutoMocker mocker)

/// <summary>
/// Gets captured metrics from the in-memory exporter.
/// </summary>
public static IList<OpenTelemetry.Metrics.Metric> GetApplicationInsightsMetrics(this AutoMocker mocker)

/// <summary>
/// Gets captured activities (traces) from the in-memory exporter.
/// </summary>
public static IList<Activity> GetApplicationInsightsActivities(this AutoMocker mocker)
```

### Application Insights 2.x

```csharp
/// <summary>
/// Sets up AutoMocker with Application Insights using a FakeTelemetryChannel.
/// </summary>
public static AutoMocker WithApplicationInsights(this AutoMocker mocker)

/// <summary>
/// Gets telemetry items captured by the FakeTelemetryChannel.
/// </summary>
public static IReadOnlyList<ITelemetry> GetSentTelemetry(this AutoMocker mocker)
```

## How It Works

The generator detects the major version of `Microsoft.ApplicationInsights` referenced by your project and generates version-appropriate code.

### Application Insights 3.x (with OpenTelemetry)

1. Creates a `TelemetryCollector` instance containing lists for log records, metrics, and activities
2. Creates a `TelemetryConfiguration` with a dummy connection string
3. Configures the OpenTelemetry builder on the configuration to export to the in-memory collector lists
4. Creates a `TelemetryClient` with the configuration
5. Registers the `TelemetryCollector`, `TelemetryConfiguration`, and `TelemetryClient` with the `AutoMocker`

The `GetApplicationInsightsLogRecords()`, `GetApplicationInsightsMetrics()`, and `GetApplicationInsightsActivities()` methods retrieve the `TelemetryCollector` from the `AutoMocker` and return the corresponding list.

### Application Insights 2.x

1. Creates a `FakeTelemetryChannel` that captures `ITelemetry` items instead of sending them
2. Creates a `TelemetryConfiguration` with the fake channel and a dummy instrumentation key
3. Creates a `TelemetryClient` with the fake configuration
4. Registers everything with the `AutoMocker`

The `GetSentTelemetry()` method retrieves the `FakeTelemetryChannel` and returns the captured items.

### Application Insights 3.x (without InMemory exporter)

Generates a method marked `[Obsolete]` that throws `InvalidOperationException`, providing a clear message to add `OpenTelemetry.Exporter.InMemory`.

## Advanced Usage

### Method Chaining

The `WithApplicationInsights()` extension method returns the `AutoMocker` instance, allowing fluent chaining:

```csharp
[TestMethod]
public void Test_CombinedSetup()
{
    AutoMocker mocker = new();

    mocker.WithApplicationInsights()
          .WithFakeLogging()
          .WithOptions<MySettings>(s => s.Value = "test");

    var service = mocker.CreateInstance<MyService>();
    // ... test assertions ...
}
```

### Combining Telemetry Types

With Application Insights 3.x, you can verify multiple types of telemetry in a single test:

```csharp
[TestMethod]
public void ComplexOperation_TracksAllTelemetry()
{
    AutoMocker mocker = new();
    mocker.WithApplicationInsights();

    var service = mocker.CreateInstance<MyService>();
    service.ComplexOperation();

    // Flush to ensure metrics are exported
    mocker.Get<TelemetryClient>().Flush();

    var logRecords = mocker.GetApplicationInsightsLogRecords();
    var metrics = mocker.GetApplicationInsightsMetrics();
    var activities = mocker.GetApplicationInsightsActivities();

    Assert.IsTrue(logRecords.Count > 0);
    Assert.IsTrue(metrics.Count > 0);
    Assert.IsTrue(activities.Count > 0);
}
```

## Disabling the Generator

You can disable this generator using an MSBuild property in your test project's `.csproj` file:

```xml
<PropertyGroup>
  <EnableMoqAutoMockerApplicationInsightsGenerator>false</EnableMoqAutoMockerApplicationInsightsGenerator>
</PropertyGroup>
```

### Example: Disabling in Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>

    <!-- Disable Application Insights Extension Generator -->
    <EnableMoqAutoMockerApplicationInsightsGenerator>false</EnableMoqAutoMockerApplicationInsightsGenerator>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq.AutoMock" Version="3.5.0" />
    <PackageReference Include="Microsoft.ApplicationInsights" Version="3.0.0" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Extension Method Not Available

1. Verify `Microsoft.ApplicationInsights` is referenced in your test project
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you're using the `Moq.AutoMock` namespace

### Obsolete Warning / InvalidOperationException with Application Insights 3.x

If you see a compiler warning about `WithApplicationInsights` being obsolete, or get an `InvalidOperationException` at runtime:

1. Add a reference to `OpenTelemetry.Exporter.InMemory` in your test project
2. Rebuild the project so the generator detects the new reference

### TelemetryClient Not Configured

1. Make sure you're calling `WithApplicationInsights()` before creating your instance
2. Verify the `TelemetryClient` is being injected properly
3. Check that you're using the same `AutoMocker` instance throughout the test

### Metrics List Is Empty

OpenTelemetry metrics require an explicit flush before they appear in the in-memory exporter. Call `mocker.Get<TelemetryClient>().Flush()` before reading metrics:

```csharp
mocker.Get<TelemetryClient>().Flush();
var metrics = mocker.GetApplicationInsightsMetrics();
```

## Best Practices

- **Always flush for metrics**: Call `mocker.Get<TelemetryClient>().Flush()` before asserting on metrics with 3.x
- **Migrate to 3.x APIs**: If upgrading from Application Insights 2.x, switch from `GetSentTelemetry()` to the type-specific methods (`GetApplicationInsightsLogRecords()`, `GetApplicationInsightsMetrics()`, `GetApplicationInsightsActivities()`)
- **Add `OpenTelemetry.Exporter.InMemory` early**: When using Application Insights 3.x, add this package to avoid the obsolete stub
