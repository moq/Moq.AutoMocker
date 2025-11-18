# Options Extension Generator

When your test project references `Microsoft.Extensions.Options`, this generator creates a `WithOptions<T>()` extension method for `AutoMocker` that simplifies testing classes that depend on `IOptions<T>`.

## Features

- Automatically generates when `Microsoft.Extensions.Options` is referenced
- Provides fluent API for configuring options in tests
- Sets up all necessary options infrastructure (`IOptionsMonitor`, `IOptionsSnapshot`, etc.)

## Usage

```csharp
using Microsoft.Extensions.Options;

public class MyService
{
    public IOptions<MySettings> Settings { get; }
    
    public MyService(IOptions<MySettings> settings)
    {
        Settings = settings;
    }
}

public class MySettings
{
    public int Number { get; set; }
    public string Required { get; set; }
}

[TestClass]
public class MyServiceTests
{
    [TestMethod]
    public void Test_WithOptions()
    {
        AutoMocker mocker = new();
        
        // Use the generated WithOptions extension method
        mocker.WithOptions<MySettings>(options => 
        {
            options.Number = 42;
            options.Required = "test value";
        });

        MyService service = mocker.CreateInstance<MyService>();

        Assert.AreEqual(42, service.Settings.Value.Number);
        Assert.AreEqual("test value", service.Settings.Value.Required);
    }
}
```

## Generated Extension Method

The generator creates:

```csharp
public static AutoMocker WithOptions<TClass>(this AutoMocker mocker, Action<TClass>? configure = null)
    where TClass : class
{
    // Sets up IOptions<T>, IOptionsMonitor<T>, IOptionsSnapshot<T>, etc.
    // Applies the configure action to the options instance
}
```

## How It Works

The `WithOptions<T>()` method sets up the complete options infrastructure in your `AutoMocker` instance:

1. Creates an `IConfigureOptions<T>` with your configuration delegate
2. Sets up `IOptionsMonitorCache<T>` using `OptionsCache<T>`
3. Sets up `IOptionsFactory<T>` using `OptionsFactory<T>`
4. Sets up `IOptionsMonitor<T>` using `OptionsMonitor<T>`
5. Sets up `IOptionsSnapshot<T>` using `OptionsManager<T>`
6. Creates an `IOptions<T>` instance with your configured values
7. Registers the configured options instance directly

This means you can inject any of these types into your classes under test:
- `IOptions<T>`
- `IOptionsSnapshot<T>`
- `IOptionsMonitor<T>`
- `IOptionsFactory<T>`
- The configuration type `T` directly

## Disabling the Generator

You can disable this generator using an MSBuild property in your test project's `.csproj` file:

```xml
<PropertyGroup>
  <EnableMoqAutoMockerOptionsGenerator>false</EnableMoqAutoMockerOptionsGenerator>
</PropertyGroup>
```

### Example: Disabling in Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    
    <!-- Disable Options Extension Generator -->
    <EnableMoqAutoMockerOptionsGenerator>false</EnableMoqAutoMockerOptionsGenerator>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq.AutoMock" Version="3.5.0" />
    <PackageReference Include="Microsoft.Extensions.Options" Version="8.0.0" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Extension Method Not Available

1. Verify `Microsoft.Extensions.Options` is referenced in your test project
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you're using the `Moq.AutoMock` namespace

### Options Not Configured Correctly

1. Make sure you're calling `WithOptions<T>()` before creating your instance
2. Verify the configuration delegate is setting the properties correctly
3. Check that you're using `Settings.Value` to access the configured options
