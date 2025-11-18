# Moq.AutoMocker Source Generators

Moq.AutoMock includes built-in source generators that provide compile-time code generation to enhance your testing experience. These generators automatically create boilerplate test code and extension methods, saving you time and reducing repetitive testing code.

## Available Generators

Moq.AutoMock includes four source generators:

### 1. [Unit Test Generator](SourceGenerators/UnitTestGenerator.md)

Automatically generates constructor null-check tests for your classes.

**Key Features:**
- Generates tests for each nullable constructor parameter
- Supports MSTest, xUnit, NUnit, and TUnit testing frameworks
- Validates `ArgumentNullException` is thrown with correct parameter names
- Provides customization hooks for test setup

**Quick Example:**
```csharp
[TestClass]
[ConstructorTests(TargetType = typeof(MyController))]
public partial class MyControllerTests
{
}
```

[Learn more →](SourceGenerators/UnitTestGenerator.md)

### 2. [Options Extension Generator](SourceGenerators/OptionsExtensionGenerator.md)

Generates `WithOptions<T>()` extension method when `Microsoft.Extensions.Options` is referenced.

**Key Features:**
- Fluent API for configuring options in tests
- Sets up complete options infrastructure (`IOptionsMonitor`, `IOptionsSnapshot`, etc.)
- Simplifies testing of classes that depend on `IOptions<T>`

**Quick Example:**
```csharp
mocker.WithOptions<MySettings>(options => 
{
    options.Number = 42;
    options.Required = "test value";
});
```

[Learn more →](SourceGenerators/OptionsExtensionGenerator.md)

### 3. [Fake Logging Extension Generator](SourceGenerators/FakeLoggingExtensionGenerator.md)

Generates `AddFakeLogging()` extension method when `Microsoft.Extensions.Diagnostics.Testing` is referenced.

**Key Features:**
- Captures log messages for verification
- Uses Microsoft's official testing helpers
- Enables logging behavior validation in tests

**Quick Example:**
```csharp
mocker.AddFakeLogging();
var provider = mocker.Get<FakeLoggerProvider>();

// ... perform actions ...

var logs = provider.Collector.GetSnapshot();
Assert.IsTrue(logs.Any(log => log.Message == "Expected message"));
```

[Learn more →](SourceGenerators/FakeLoggingExtensionGenerator.md)

### 4. [Application Insights Extension Generator](SourceGenerators/ApplicationInsightsExtensionGenerator.md)

Generates `WithApplicationInsights()` extension method when `Microsoft.ApplicationInsights` is referenced.

**Key Features:**
- Provides `TelemetryClient` that doesn't make real API calls
- Enables testing of telemetry tracking code
- No cloud dependencies in unit tests

**Quick Example:**
```csharp
mocker.WithApplicationInsights();
var service = mocker.CreateInstance<MyService>();
service.DoWork(); // TelemetryClient calls won't hit the cloud
```

[Learn more →](SourceGenerators/ApplicationInsightsExtensionGenerator.md)

## Tips and Best Practices

### Review Generated Code

To see the generated code, enable source generators output in your `.csproj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```
