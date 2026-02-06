# Moq.AutoMocker Source Generators

Moq.AutoMock includes built-in source generators that provide compile-time code generation to enhance your testing experience. These generators automatically create boilerplate test code and extension methods, saving you time and reducing repetitive testing code.

## Available Generators

Moq.AutoMock includes several source generators:

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

Generates `WithFakeLogging()` extension method when `Microsoft.Extensions.Diagnostics.Testing` is referenced.

**Key Features:**
- Captures log messages for verification
- Uses Microsoft's official testing helpers
- Enables logging behavior validation in tests

**Quick Example:**
```csharp
mocker.WithFakeLogging();
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

### 5. [Keyed Services Extension Generator](SourceGenerators/KeyedServicesExtensionGenerator.md)

Generates `WithKeyedService<T>()` extension methods when `Microsoft.Extensions.DependencyInjection` is referenced.

**Key Features:**
- Enables testing of keyed services via `IKeyedServiceProvider`
- Supports `[FromKeyedServices]` attribute resolution
- Provides both eager and lazy service registration
- Integrates seamlessly with dependency injection

**Quick Example:**
```csharp
mocker.WithKeyedService(Mock.Of<IEmailSender>(), "primary");
mocker.WithKeyedService<ICache, RedisCache>("cache");

var service = mocker.CreateInstance<MyService>();
// Keyed services are automatically resolved
```

[Learn more →](SourceGenerators/KeyedServicesExtensionGenerator.md)

## Important: Generated Classes Are Internal Partials

All extension classes created by these source generators are generated as **partial classes** with **internal visibility**. For example, the Keyed Services generator produces:

```csharp
internal static partial class AutoMockerKeyedServicesExtensions
{
    // Generated extension methods...
}
```

### Ambiguous Reference Issues with Multiple Test Projects

This design can cause **ambiguous method call errors** when multiple test projects reference each other and both have the source generators enabled. Since each project generates its own internal partial class with the same name and methods, projects that share visibility (e.g., via `InternalsVisibleTo` or project references) may see duplicate definitions.

**Example error:**
```
The call is ambiguous between the following methods or properties:
'Moq.AutoMock.AutoMockerKeyedServicesExtensions.WithKeyedService(...)' and
'Moq.AutoMock.AutoMockerKeyedServicesExtensions.WithKeyedService(...)'
```

This commonly occurs when:
- An integration test project references a unit test project
- `InternalsVisibleTo` is used between test projects
- Both projects reference `Moq.AutoMock` and the same packages that trigger generators (e.g., `Microsoft.Extensions.DependencyInjection.Abstractions`)

**Solution:** Disable the source generator in one of the projects (see [Disabling Source Generators](#disabling-source-generators) below).

For more details, see [Issue #410](https://github.com/moq/Moq.AutoMocker/issues/410).

## Disabling Source Generators

Each source generator can be individually disabled using MSBuild properties in your project's `.csproj` file:

| Generator | MSBuild Property |
|-----------|-----------------|
| Options Extension | `EnableMoqAutoMockerOptionsGenerator` |
| Keyed Services Extension | `EnableMoqAutoMockerKeyedServicesGenerator` |
| Fake Logging Extension | `EnableMoqAutoMockerFakeLoggingGenerator` |
| Application Insights Extension | `EnableMoqAutoMockerApplicationInsightsGenerator` |

**Example: Disabling a generator**
```xml
<PropertyGroup>
  <!-- Disable the Keyed Services generator -->
  <EnableMoqAutoMockerKeyedServicesGenerator>false</EnableMoqAutoMockerKeyedServicesGenerator>
</PropertyGroup>
```

**Example: Disabling multiple generators**
```xml
<PropertyGroup>
  <EnableMoqAutoMockerKeyedServicesGenerator>false</EnableMoqAutoMockerKeyedServicesGenerator>
  <EnableMoqAutoMockerOptionsGenerator>false</EnableMoqAutoMockerOptionsGenerator>
</PropertyGroup>
```

## Tips and Best Practices

### Review Generated Code

To see the generated code, enable source generators output in your `.csproj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```
