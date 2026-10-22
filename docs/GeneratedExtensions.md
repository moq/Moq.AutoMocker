# Generated extensions

Namespace: `Moq.AutoMock`. [Documentation index](README.md).

These APIs are generated into the consuming test project only when their prerequisite packages are referenced. Call them before resolving affected dependencies. Extension receivers (`this AutoMocker mocker`) are omitted below.

| Method | Required reference | Result / details |
| --- | --- | --- |
| `AutoMocker WithOptions<TClass>(Action<TClass>? configure = null) where TClass : class` | `Microsoft.Extensions.Options` | Registers options infrastructure and the configured options value; [guide](SourceGenerators/OptionsExtensionGenerator.md) |
| `AutoMocker WithFakeLogging()` | `Microsoft.Extensions.Diagnostics.Testing` | Resolves logging services through `FakeLoggerProvider`; [guide](SourceGenerators/FakeLoggingExtensionGenerator.md) |
| `AutoMocker WithFakeTimeProvider()` | `Microsoft.Extensions.TimeProvider.Testing` | Registers one `FakeTimeProvider` as itself and `TimeProvider`; [guide](SourceGenerators/FakeTimeProviderExtensionGenerator.md) |
| `AutoMocker WithMeterFactory()` | `System.Diagnostics.DiagnosticSource` 10+ | Registers a real `IMeterFactory` that tracks created meters; [guide](SourceGenerators/MeterFactoryExtensionGenerator.md) |
| `AutoMocker WithHttpClientFactory()` | `Microsoft.Extensions.Http` | Registers `IHttpClientFactory`, caching one client per name with a shared testable handler; [guide](SourceGenerators/HttpClientFactoryExtensionGenerator.md) |
| `AutoMocker WithApplicationInsights()` | `Microsoft.ApplicationInsights` | Configures captured telemetry rather than network export; [guide](SourceGenerators/ApplicationInsightsExtensionGenerator.md) |

All methods in this table return the same container for chaining.

```csharp
var mocker = new AutoMocker()
    .WithOptions<MySettings>(settings => settings.Name = "test")
    .WithFakeLogging()
    .WithFakeTimeProvider()
    .WithMeterFactory()
    .WithHttpClientFactory();
```

Each call requires the corresponding reference. See the linked guides for package namespaces and examples reading logs, controlling time, recording metrics, and configuring clients.

## WithKeyedService overloads

Requires `Microsoft.Extensions.DependencyInjection.Abstractions` 8+.

```csharp
void WithKeyedService<TService>(TService service, object? key)
    where TService : class
void WithKeyedService<TService, TImplementation>(TImplementation service, object? key)
    where TImplementation : class, TService
void WithKeyedService<TService>(object? key) where TService : class
void WithKeyedService<TService, TImplementation>(object? key)
    where TImplementation : class, TService
```

Instance overloads register immediately. Key-only overloads lazily construct and cache the implementation using `CreateInstance`; the one-type form therefore needs a constructible class. Methods return `void`, not the container. Services resolve through `IKeyedServiceProvider` or `[FromKeyedServices]` constructor parameters.

```csharp
mocker.WithKeyedService<IStore>(new Store(), "primary");
mocker.WithKeyedService<IStore, Store>("secondary");
mocker.WithKeyedService<Store>("concrete");
```

See [keyed services](SourceGenerators/KeyedServicesExtensionGenerator.md) for retrieval and constructor injection.

## Application Insights accessors

Call `WithApplicationInsights()` first. Application Insights 2.x generates:

```csharp
IReadOnlyList<ITelemetry> GetSentTelemetry()
```

```csharp
mocker.WithApplicationInsights();
mocker.Get<TelemetryClient>().TrackEvent("test");
var sent = mocker.GetSentTelemetry();
```

The generated internal partial `AutoMockerApplicationInsightsExtensions` also contains public nested `FakeTelemetryChannel()`: `DeveloperMode` (`bool?`, get/set), `EndpointAddress` (`string?`, get/set), and `SentItems` (`IReadOnlyList<ITelemetry>`, get). `Send(ITelemetry)` records non-null items; `Flush()` and `Dispose()` are no-ops. Prefer the accessor above.

Application Insights 3.x with `OpenTelemetry.Exporter.InMemory` generates:

```csharp
IList<LogRecord> GetApplicationInsightsLogRecords()
IList<OpenTelemetry.Metrics.Metric> GetApplicationInsightsMetrics()
IList<Activity> GetApplicationInsightsActivities()
```

```csharp
mocker.WithApplicationInsights();
// Exercise the service, then flush before reading metrics.
mocker.Get<TelemetryClient>().Flush();
var logs = mocker.GetApplicationInsightsLogRecords();
var metrics = mocker.GetApplicationInsightsMetrics();
var activities = mocker.GetApplicationInsightsActivities();
```

Without the in-memory exporter, the 3.x generator emits an obsolete `WithApplicationInsights()` stub that throws `InvalidOperationException`.

## Configuration and visibility

Most extension classes are internal partials. `AutoMockerHttpClientFactoryExtensions` is public. Avoid enabling the same generated extensions in mutually visible test projects; see [generator configuration](SourceGenerators.md#disabling-source-generators).

Constructor-test attributes are covered in [Diagnostics](Diagnostics.md#constructortestsattribute); Roslyn host entry points are covered in [Compiler integration](CompilerIntegration.md).
