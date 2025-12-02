# Keyed Services Extension Generator

When your test project references `Microsoft.Extensions.DependencyInjection.Abstractions` version 8.x or newer, this generator creates `WithKeyedService<T>()` extension methods for `AutoMocker` that enable testing classes that depend on keyed services via `IKeyedServiceProvider`.

## Features

- Automatically generates when `Microsoft.Extensions.DependencyInjection` is referenced
- Supports registering services with specific keys
- Provides both eager and lazy service registration
- Automatically resolves `[FromKeyedServices]` attribute parameters
- Integrates seamlessly with `IServiceProvider` and `IKeyedServiceProvider`

## What are Keyed Services?

Keyed services are a feature in .NET's dependency injection that allows you to register multiple implementations of the same interface and distinguish them using keys. This is useful when you need different implementations of the same service type in different contexts.

## Usage

### Basic Keyed Service Registration

Register a service instance with a specific key:

```csharp
using Microsoft.Extensions.DependencyInjection;

public class MyService
{
    public IEmailSender EmailSender { get; }
    
    public MyService([FromKeyedServices("primary")] IEmailSender emailSender)
    {
        EmailSender = emailSender;
    }
}

[TestClass]
public class MyServiceTests
{
    [TestMethod]
    public void Test_WithKeyedService()
    {
        AutoMocker mocker = new();
        
        // Register a keyed service
        IEmailSender primarySender = Mock.Of<IEmailSender>();
        mocker.WithKeyedService(primarySender, "primary");

        MyService service = mocker.CreateInstance<MyService>();

        Assert.AreEqual(primarySender, service.EmailSender);
    }
}
```

### Multiple Keyed Services

Register multiple services with different keys:

```csharp
public class NotificationService
{
    public IEmailSender PrimarySender { get; }
    public IEmailSender BackupSender { get; }
    
    public NotificationService(
        [FromKeyedServices("primary")] IEmailSender primarySender,
        [FromKeyedServices("backup")] IEmailSender backupSender)
    {
        PrimarySender = primarySender;
        BackupSender = backupSender;
    }
}

[TestMethod]
public void Test_MultipleKeyedServices()
{
    AutoMocker mocker = new();
    
    // Register multiple keyed services
    IEmailSender primary = Mock.Of<IEmailSender>();
    IEmailSender backup = Mock.Of<IEmailSender>();
    mocker.WithKeyedService(primary, "primary");
    mocker.WithKeyedService(backup, "backup");

    NotificationService service = mocker.CreateInstance<NotificationService>();

    Assert.AreEqual(primary, service.PrimarySender);
    Assert.AreEqual(backup, service.BackupSender);
}
```

### Using IKeyedServiceProvider

Access keyed services through `IKeyedServiceProvider`:

```csharp
public class ServiceConsumer
{
    public IService Service { get; }
    
    public ServiceConsumer(IServiceProvider serviceProvider)
    {
        Service = serviceProvider.GetRequiredKeyedService<IService>("my-key");
    }
}

[TestMethod]
public void Test_IKeyedServiceProvider()
{
    AutoMocker mocker = new();
    
    IService myService = Mock.Of<IService>();
    mocker.WithKeyedService(myService, "my-key");

    ServiceConsumer consumer = mocker.CreateInstance<ServiceConsumer>();

    Assert.AreEqual(myService, consumer.Service);
}
```

### Lazy Service Registration

Register a keyed service that will be created by AutoMocker when first accessed:

```csharp
public interface ICache { }
public class RedisCache : ICache { }

public class MyService
{
    public ICache Cache { get; }
    
    public MyService([FromKeyedServices("redis")] ICache cache)
    {
        Cache = cache;
    }
}

[TestMethod]
public void Test_LazyKeyedService()
{
    AutoMocker mocker = new();
    
    // Register a keyed service that will be created lazily
    mocker.WithKeyedService<ICache, RedisCache>("redis");

    MyService service = mocker.CreateInstance<MyService>();

    Assert.IsNotNull(service.Cache);
    Assert.IsInstanceOfType(service.Cache, typeof(RedisCache));
}
```

## Generated Extension Methods

The generator creates the following extension methods:

### WithKeyedService (with instance)

```csharp
public static void WithKeyedService<TService>(this AutoMocker mocker, TService service, object? key)
    where TService : class
{
    // Registers the provided service instance with the specified key
}

public static void WithKeyedService<TService, TImplementation>(this AutoMocker mocker, TImplementation service, object? key)
    where TImplementation : class, TService
{
    // Registers the provided service instance with the specified key and service type
}
```

### WithKeyedService (lazy)

```csharp
public static void WithKeyedService<TService>(this AutoMocker mocker, object? key)
    where TService : class
{
    // Registers a keyed service that will be created using AutoMocker when requested
}

public static void WithKeyedService<TService, TImplementation>(this AutoMocker mocker, object? key)
    where TImplementation : class, TService
{
    // Registers a keyed service that will be created using AutoMocker when requested
}
```

## How It Works

The generator:

1. Creates a `ServiceProviderResolver` that intercepts dependency resolution
2. Implements both `IServiceProvider` and `IKeyedServiceProvider` interfaces
3. Automatically detects `[FromKeyedServices]` attributes on constructor parameters
4. Resolves keyed services from a centralized registry
5. Falls back to AutoMocker's default resolution for non-keyed services

## Advanced Usage

### Different Service and Implementation Types

```csharp
public interface IRepository { }
public class SqlRepository : IRepository { }
public class MongoRepository : IRepository { }

[TestMethod]
public void Test_DifferentImplementations()
{
    AutoMocker mocker = new();
    
    // Register different implementations for the same interface
    mocker.WithKeyedService<IRepository, SqlRepository>("sql");
    mocker.WithKeyedService<IRepository, MongoRepository>("mongo");

    var sqlRepo = mocker.Get<IServiceProvider>()
        .GetRequiredKeyedService<IRepository>("sql");
    var mongoRepo = mocker.Get<IServiceProvider>()
        .GetRequiredKeyedService<IRepository>("mongo");

    Assert.IsInstanceOfType(sqlRepo, typeof(SqlRepository));
    Assert.IsInstanceOfType(mongoRepo, typeof(MongoRepository));
}
```

### Keyed Services with Dependencies

Lazy-registered keyed services can have their own dependencies resolved by AutoMocker:

```csharp
public class CacheService : ICache
{
    public ILogger<CacheService> Logger { get; }
    
    public CacheService(ILogger<CacheService> logger)
    {
        Logger = logger;
    }
}

[TestMethod]
public void Test_KeyedServiceWithDependencies()
{
    AutoMocker mocker = new();
    
    // The CacheService will be created with its logger dependency automatically mocked
    mocker.WithKeyedService<ICache, CacheService>("primary");

    var cache = mocker.Get<IServiceProvider>()
        .GetRequiredKeyedService<ICache>("primary");

    Assert.IsNotNull(cache);
    Assert.IsInstanceOfType(cache, typeof(CacheService));
    
    // Dependencies are automatically resolved
    var cacheService = (CacheService)cache;
    Assert.IsNotNull(cacheService.Logger);
}
```

### Null Service Keys

You can use `null` as a service key:

```csharp
[TestMethod]
public void Test_NullKey()
{
    AutoMocker mocker = new();
    
    IService service = Mock.Of<IService>();
    mocker.WithKeyedService(service, null);

    var resolved = mocker.Get<IServiceProvider>()
        .GetKeyedService<IService>(null);

    Assert.AreEqual(service, resolved);
}
```

## Disabling the Generator

You can disable this generator using an MSBuild property in your test project's `.csproj` file:

```xml
<PropertyGroup>
  <EnableMoqAutoMockerKeyedServicesGenerator>false</EnableMoqAutoMockerKeyedServicesGenerator>
</PropertyGroup>
```

### Example: Disabling in Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    
    <!-- Disable Keyed Services Extension Generator -->
    <EnableMoqAutoMockerKeyedServicesGenerator>false</EnableMoqAutoMockerKeyedServicesGenerator>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq.AutoMock" Version="3.5.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Extension Method Not Available

1. Verify `Microsoft.Extensions.DependencyInjection` is referenced in your test project
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you're using the `Moq.AutoMock` namespace

### Keyed Service Not Resolved

1. Make sure you're calling `WithKeyedService()` before creating your instance
2. Verify the service key matches exactly (including type and value)
3. Check that the constructor parameter has the `[FromKeyedServices]` attribute
4. Ensure you're accessing via `IKeyedServiceProvider` or `[FromKeyedServices]` parameter

### Service Not Found Exception

If you get an "Failed to resolve keyed service" exception:

1. Verify the service was registered with the correct key
2. Check the service type matches the requested type
3. For lazy registration, ensure the implementation type can be created by AutoMocker
4. Confirm you're using `GetRequiredKeyedService` or `GetKeyedService` with the right key

## Best Practices

### Use Strongly-Typed Keys

Consider using constants or enums for service keys to avoid typos:

```csharp
public static class ServiceKeys
{
    public const string Primary = "primary";
    public const string Backup = "backup";
    public const string Cache = "cache";
}

[TestMethod]
public void Test_WithConstants()
{
    AutoMocker mocker = new();
    mocker.WithKeyedService(Mock.Of<IService>(), ServiceKeys.Primary);
    
    // Use the same constant when accessing
    var service = mocker.Get<IServiceProvider>()
        .GetRequiredKeyedService<IService>(ServiceKeys.Primary);
}
```

### Prefer Lazy Registration for Complex Objects

Use lazy registration when the service has dependencies:

```csharp
// Prefer this - dependencies are automatically resolved
mocker.WithKeyedService<ICache, CacheService>("cache");

// Over this - you have to manually set up dependencies
var cache = new CacheService(Mock.Of<ILogger<CacheService>>());
mocker.WithKeyedService<ICache>(cache, "cache");
```

### Combine with Other Generators

Keyed services work well with other generator features:

```csharp
[TestMethod]
public void Test_CombinedGenerators()
{
    AutoMocker mocker = new();
    
    mocker.AddFakeLogging()
          .WithOptions<MySettings>(s => s.Timeout = 30);
    
    mocker.WithKeyedService<ICache, RedisCache>("redis");
    
    var service = mocker.CreateInstance<MyComplexService>();
    // All dependencies including keyed services are resolved
}
```
