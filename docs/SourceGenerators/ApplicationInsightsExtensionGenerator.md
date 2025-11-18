# Application Insights Extension Generator

When your test project references `Microsoft.ApplicationInsights`, this generator creates a `WithApplicationInsights()` extension method for testing telemetry tracking.

## Features

- Automatically generates when `Microsoft.ApplicationInsights` is referenced
- Provides a `TelemetryClient` that doesn't make real API calls
- Allows verification of telemetry calls in tests

## Usage

```csharp
using Microsoft.ApplicationInsights;

public class MyService
{
    private readonly TelemetryClient _telemetryClient;
    
    public MyService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }
    
    public void DoWork()
    {
        _telemetryClient.TrackEvent("WorkStarted");
        // ... do work ...
        _telemetryClient.TrackEvent("WorkCompleted");
    }
}

[TestClass]
public class MyServiceTests
{
    [TestMethod]
    public void Test_WithApplicationInsights()
    {
        AutoMocker mocker = new();
        
        // Use the generated WithApplicationInsights extension method
        mocker.WithApplicationInsights();

        MyService service = mocker.CreateInstance<MyService>();
        service.DoWork();
        
        // TelemetryClient was properly injected and won't make real API calls
        Assert.IsNotNull(service);
    }
}
```

## Generated Extension Method

The generator creates:

```csharp
public static AutoMocker WithApplicationInsights(this AutoMocker mocker)
{
    // Sets up TelemetryClient with a fake channel that doesn't send telemetry
    // Returns the AutoMocker for method chaining
}
```

## How It Works

The `WithApplicationInsights()` method:

1. Creates a `FakeTelemetryChannel` that doesn't send telemetry to the cloud
2. Creates a `TelemetryConfiguration` with the fake channel
3. Sets a dummy instrumentation key in the configuration
4. Creates a `TelemetryClient` with the fake configuration
5. Registers the `TelemetryClient` instance with the `AutoMocker`

This ensures:
- Your tests don't make real API calls to Application Insights
- The `TelemetryClient` is properly configured and functional
- You can test code that depends on Application Insights without side effects

## Advanced Usage

### Method Chaining

The extension method returns the `AutoMocker` instance, allowing for fluent chaining:

```csharp
[TestMethod]
public void Test_CombinedSetup()
{
    AutoMocker mocker = new();
    
    mocker.WithApplicationInsights()
          .AddFakeLogging()
          .WithOptions<MySettings>(s => s.Value = "test");
    
    var service = mocker.CreateInstance<MyService>();
    // ... test assertions ...
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
    <TargetFramework>net8.0</TargetFramework>
    
    <!-- Disable Application Insights Extension Generator -->
    <EnableMoqAutoMockerApplicationInsightsGenerator>false</EnableMoqAutoMockerApplicationInsightsGenerator>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq.AutoMock" Version="3.5.0" />
    <PackageReference Include="Microsoft.ApplicationInsights" Version="2.22.0" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Extension Method Not Available

1. Verify `Microsoft.ApplicationInsights` is referenced in your test project
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you're using the `Moq.AutoMock` namespace

### TelemetryClient Not Configured

1. Make sure you're calling `WithApplicationInsights()` before creating your instance
2. Verify the `TelemetryClient` is being injected properly
3. Check that you're using the same `AutoMocker` instance throughout the test
