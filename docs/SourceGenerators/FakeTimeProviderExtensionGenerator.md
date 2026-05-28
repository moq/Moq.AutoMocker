# Fake Time Provider Extension Generator

When your test project references `Microsoft.Extensions.TimeProvider.Testing`, this generator creates a `WithFakeTimeProvider()` extension method for `AutoMocker` that sets up a `FakeTimeProvider` for deterministic time-based testing.

## Features

- Automatically generates when `Microsoft.Extensions.TimeProvider.Testing` is referenced
- Registers `FakeTimeProvider` as both `TimeProvider` (for constructor injection) and `FakeTimeProvider` (for test control)
- Enables deterministic time manipulation with `Advance()`, `SetUtcNow()`, and `AutoAdvanceAmount`
- Uses Microsoft's official `FakeTimeProvider` from `Microsoft.Extensions.Time.Testing`

## Installation

Install the `Microsoft.Extensions.TimeProvider.Testing` package:

```bash
dotnet add package Microsoft.Extensions.TimeProvider.Testing
```

## Usage

```csharp
using Microsoft.Extensions.Time.Testing;

public class SchedulerService
{
    private readonly TimeProvider _timeProvider;

    public SchedulerService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public bool IsBusinessHours()
    {
        var now = _timeProvider.GetLocalNow();
        return now.Hour >= 9 && now.Hour < 17;
    }
}

[TestClass]
public class SchedulerServiceTests
{
    [TestMethod]
    public void IsBusinessHours_DuringBusinessDay_ReturnsTrue()
    {
        // Arrange
        AutoMocker mocker = new();
        mocker.WithFakeTimeProvider();

        var fakeTime = mocker.Get<FakeTimeProvider>();
        fakeTime.SetUtcNow(new DateTimeOffset(2025, 1, 13, 14, 0, 0, TimeSpan.Zero)); // 2 PM UTC

        // Act
        SchedulerService service = mocker.CreateInstance<SchedulerService>();

        // Assert
        Assert.IsTrue(service.IsBusinessHours());
    }

    [TestMethod]
    public void IsBusinessHours_AfterHours_ReturnsFalse()
    {
        // Arrange
        AutoMocker mocker = new();
        mocker.WithFakeTimeProvider();

        var fakeTime = mocker.Get<FakeTimeProvider>();
        fakeTime.SetUtcNow(new DateTimeOffset(2025, 1, 13, 20, 0, 0, TimeSpan.Zero)); // 8 PM UTC

        // Act
        SchedulerService service = mocker.CreateInstance<SchedulerService>();

        // Assert
        Assert.IsFalse(service.IsBusinessHours());
    }
}
```

## Generated Extension Method

The generator creates:

```csharp
public static AutoMocker WithFakeTimeProvider(this AutoMocker mocker)
{
    var timeProvider = new FakeTimeProvider();
    mocker.Use<TimeProvider>(timeProvider);
    mocker.Use(timeProvider);
    return mocker;
}
```

## How It Works

The `WithFakeTimeProvider()` method:

1. Creates a `FakeTimeProvider` instance
2. Registers it as `TimeProvider` so classes with `TimeProvider` constructor parameters receive it automatically
3. Registers it as `FakeTimeProvider` so tests can retrieve and control it via `mocker.Get<FakeTimeProvider>()`

Both registrations point to the same instance, ensuring that the time your test controls is the same time the service under test observes.

## Advanced Usage

### Advancing Time

```csharp
[TestMethod]
public void Cache_AfterExpiry_RemovesEntry()
{
    // Arrange
    AutoMocker mocker = new();
    mocker.WithFakeTimeProvider();

    var fakeTime = mocker.Get<FakeTimeProvider>();
    fakeTime.SetUtcNow(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));

    ExpiryCache cache = mocker.CreateInstance<ExpiryCache>();
    cache.Add("key", "value");

    // Act — advance past expiry
    fakeTime.Advance(TimeSpan.FromMinutes(10));

    // Assert
    Assert.IsFalse(cache.TryGetValue("key", out _));
}
```

### Auto-Advancing Time

`FakeTimeProvider` supports automatically advancing time on each `GetUtcNow()` call:

```csharp
[TestMethod]
public void Service_WithAutoAdvance_ObservesPassingTime()
{
    AutoMocker mocker = new();
    mocker.WithFakeTimeProvider();

    var fakeTime = mocker.Get<FakeTimeProvider>();
    fakeTime.AutoAdvanceAmount = TimeSpan.FromSeconds(1);

    TimestampService service = mocker.CreateInstance<TimestampService>();

    var first = service.GetTimestamp();
    var second = service.GetTimestamp();

    Assert.IsTrue(second > first);
}
```

### Chaining with Other Extensions

`WithFakeTimeProvider()` returns the `AutoMocker` instance for fluent chaining:

```csharp
AutoMocker mocker = new AutoMocker()
    .WithFakeTimeProvider()
    .WithFakeLogging();
```

## Disabling the Generator

You can disable this generator using an MSBuild property in your test project's `.csproj` file:

```xml
<PropertyGroup>
  <EnableMoqAutoMockerFakeTimeProviderGenerator>false</EnableMoqAutoMockerFakeTimeProviderGenerator>
</PropertyGroup>
```

### Example: Disabling in Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>

    <!-- Disable Fake Time Provider Extension Generator -->
    <EnableMoqAutoMockerFakeTimeProviderGenerator>false</EnableMoqAutoMockerFakeTimeProviderGenerator>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq.AutoMock" Version="3.5.0" />
    <PackageReference Include="Microsoft.Extensions.TimeProvider.Testing" Version="10.0.0" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Extension Method Not Available

1. Verify `Microsoft.Extensions.TimeProvider.Testing` is referenced in your test project
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you are using the `Moq.AutoMock` namespace

### `TimeProvider` Not Resolved

The `WithFakeTimeProvider()` call must happen before `CreateInstance<T>()`:

```csharp
// ✅ Correct — configure before creating the instance
mocker.WithFakeTimeProvider();
var service = mocker.CreateInstance<MyService>();

// ❌ Incorrect — too late, the instance already has a mocked TimeProvider
var service = mocker.CreateInstance<MyService>();
mocker.WithFakeTimeProvider();
```

### Accessing `FakeTimeProvider` for Time Control

Use `mocker.Get<FakeTimeProvider>()` to retrieve the instance registered by `WithFakeTimeProvider()`:

```csharp
mocker.WithFakeTimeProvider();
var fakeTime = mocker.Get<FakeTimeProvider>();
fakeTime.Advance(TimeSpan.FromDays(1));
```

## See Also

- [Source Generators Overview](../SourceGenerators.md)
- [Fake Logging Extension Generator](FakeLoggingExtensionGenerator.md)
- [Microsoft.Extensions.TimeProvider.Testing on NuGet](https://www.nuget.org/packages/Microsoft.Extensions.TimeProvider.Testing)
- [FakeTimeProvider API documentation](https://learn.microsoft.com/dotnet/api/microsoft.extensions.time.testing.faketimeprovider)
