# Fake Logging Extension Generator

When your test project references `Microsoft.Extensions.Diagnostics.Testing`, this generator creates an `WithFakeLogging()` extension method that sets up fake logging for testing.

## Features

- Automatically generates when `Microsoft.Extensions.Diagnostics.Testing` is referenced
- Provides access to logged messages for verification
- Uses Microsoft's official testing helpers

## Installation

Install the Microsoft.Extensions.Diagnostics.Testing package:

```bash
dotnet add package Microsoft.Extensions.Diagnostics.Testing
```

## Usage

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

public class MyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public void DoWork()
    {
        _logger.LogInformation("Starting work");
        _logger.LogDebug("Debug message");
        _logger.LogWarning("Warning message");
    }
}

[TestClass]
public class MyServiceTests
{
    [TestMethod]
    public void Test_WithFakeLogging()
    {
        AutoMocker mocker = new();
        
        // Use the generated WithFakeLogging extension method
        mocker.WithFakeLogging();
        var provider = mocker.Get<FakeLoggerProvider>();

        MyService service = mocker.CreateInstance<MyService>();
        service.DoWork();

        // Verify logged messages
        var logs = provider.Collector.GetSnapshot();
        Assert.IsTrue(logs.Any(log => log.Message == "Starting work"));
        Assert.IsTrue(logs.Any(log => log.Message == "Debug message"));
        Assert.IsTrue(logs.Any(log => log.Message == "Warning message"));
    }
}
```

## Generated Extension Method

The generator creates:

```csharp
public static AutoMocker WithFakeLogging(this AutoMocker mocker)
{
    // Sets up FakeLoggerProvider for capturing log messages
    // Configures AutoMocker to use fake loggers
}
```

## How It Works

The `WithFakeLogging()` method:

1. Creates a `FakeLoggerProvider` instance
2. Registers a custom `FakeLoggerResolver` with the `AutoMocker`
3. Inserts the resolver after the `CacheResolver` in the resolver chain
4. This ensures all `ILogger<T>` instances use the fake logger infrastructure

The `FakeLoggerProvider` captures all log messages, allowing you to:
- Verify that specific messages were logged
- Check log levels
- Inspect logged data and state
- Validate logging behavior in your tests

## Advanced Usage

### Testing Different Log Levels

```csharp
[TestMethod]
public void Test_LogLevels()
{
    AutoMocker mocker = new();
    mocker.WithFakeLogging();
    var provider = mocker.Get<FakeLoggerProvider>();

    MyService service = mocker.CreateInstance<MyService>();
    service.DoWork();

    var logs = provider.Collector.GetSnapshot();
    
    // Check for specific log levels
    Assert.IsTrue(logs.Any(log => log.Level == LogLevel.Information));
    Assert.IsTrue(logs.Any(log => log.Level == LogLevel.Debug));
    Assert.IsTrue(logs.Any(log => log.Level == LogLevel.Warning));
}
```

### Accessing ILoggerFactory

```csharp
[TestMethod]
public void Test_LoggerFactory()
{
    AutoMocker mocker = new();
    mocker.WithFakeLogging();

    // ILoggerFactory is also available
    var factory = mocker.Get<ILoggerFactory>();
    var logger = factory.CreateLogger("CustomCategory");
    
    logger.LogInformation("Test message");
    
    var provider = mocker.Get<FakeLoggerProvider>();
    var logs = provider.Collector.GetSnapshot();
    Assert.IsTrue(logs.Any(log => log.Message == "Test message"));
}
```

## Disabling the Generator

You can disable this generator using an MSBuild property in your test project's `.csproj` file:

```xml
<PropertyGroup>
  <EnableMoqAutoMockerFakeLoggingGenerator>false</EnableMoqAutoMockerFakeLoggingGenerator>
</PropertyGroup>
```

### Example: Disabling in Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    
    <!-- Disable Fake Logging Extension Generator -->
    <EnableMoqAutoMockerFakeLoggingGenerator>false</EnableMoqAutoMockerFakeLoggingGenerator>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq.AutoMock" Version="3.5.0" />
    <PackageReference Include="Microsoft.Extensions.Diagnostics.Testing" Version="8.0.0" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Extension Method Not Available

1. Verify `Microsoft.Extensions.Diagnostics.Testing` is referenced in your test project
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you're using the `Moq.AutoMock` namespace

### Logs Not Being Captured

1. Make sure you're calling `WithFakeLogging()` before creating your instance
2. Verify you're getting the `FakeLoggerProvider` from the same `AutoMocker` instance
3. Check that your service is actually using the injected logger
