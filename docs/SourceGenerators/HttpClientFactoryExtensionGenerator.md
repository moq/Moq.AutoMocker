# HTTP Client Factory Extension Generator

When your test project references `Microsoft.Extensions.Http`, this generator creates a `WithHttpClientFactory()` extension method for `AutoMocker` that provides a testable `IHttpClientFactory` implementation.

## Features

- Automatically generates when `Microsoft.Extensions.Http` is referenced
- Provides a testable `IHttpClientFactory` backed by the same `Mock<HttpMessageHandler>` used by `HttpClientResolver`
- Caches one `HttpClient` per client name: calling `CreateClient` with the same name returns the same instance, different names return distinct instances
- Existing `SetupHttp*` and `VerifyHttp*` helpers apply to all named clients, since they share the same testable handler

## Usage

```csharp
public class CatalogService
{
    private readonly HttpClient _client;

    public CatalogService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient("catalog");
    }

    public Task<HttpResponseMessage> GetProductAsync(string id)
        => _client.GetAsync($"/products/{id}");
}

[TestClass]
public class CatalogServiceTests
{
    [TestMethod]
    public async Task Test_WithHttpClientFactory()
    {
        // Arrange
        AutoMocker mocker = new();
        mocker.WithHttpClientFactory();
        mocker.SetupHttpGet("/products/1")
            .ReturnsHttpResponse(HttpStatusCode.OK, """{"id": "1"}""");

        // Act
        var service = mocker.CreateInstance<CatalogService>();
        var response = await service.GetProductAsync("1");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## Generated Extension Method

The generator creates:

```csharp
public static AutoMocker WithHttpClientFactory(this AutoMocker mocker)
{
    // Registers a resolver that provides IHttpClientFactory
    // Returns mocker for fluent chaining
}
```

## How It Works

The `WithHttpClientFactory()` method:

1. Inserts a resolver after the `CacheResolver` in the resolver chain
2. When `IHttpClientFactory` is requested, the resolver returns a testable factory implementation
3. Calling `CreateClient(name)` on that factory looks up (or creates) an `HttpClient` for the given name in an internal `ConcurrentDictionary<string, HttpClient>`, keyed with ordinal string comparison
4. Every cached `HttpClient` wraps the same `Mock<HttpMessageHandler>` used by `HttpClientResolver`, so requests made through any named client are visible to the existing `SetupHttp*`/`VerifyHttp*` helpers

This ensures any class depending on `IHttpClientFactory` receives a working implementation, without requiring a full dependency injection container, and without needing separate setup per client name.

## Advanced Usage

### Same Name Returns the Same Client

```csharp
[TestMethod]
public void Test_SameNameReturnsSameClient()
{
    AutoMocker mocker = new();
    mocker.WithHttpClientFactory();
    var factory = mocker.Get<IHttpClientFactory>();

    var first = factory.CreateClient("catalog");
    var second = factory.CreateClient("catalog");

    Assert.AreSame(first, second);
}
```

### Different Names Return Different Clients

```csharp
[TestMethod]
public void Test_DifferentNamesReturnDifferentClients()
{
    AutoMocker mocker = new();
    mocker.WithHttpClientFactory();
    var factory = mocker.Get<IHttpClientFactory>();

    var catalog = factory.CreateClient("catalog");
    var orders = factory.CreateClient("orders");

    Assert.AreNotSame(catalog, orders);
}
```

### Combining with Other Extensions

```csharp
[TestMethod]
public void Test_CombinedExtensions()
{
    AutoMocker mocker = new();
    mocker.WithHttpClientFactory()
          .WithFakeLogging();

    var service = mocker.CreateInstance<CatalogService>();
    // Verify both HTTP and logging behavior
}
```

## Disabling the Generator

You can disable this generator using an MSBuild property in your test project's `.csproj` file:

```xml
<PropertyGroup>
  <EnableMoqAutoMockerHttpClientFactoryGenerator>false</EnableMoqAutoMockerHttpClientFactoryGenerator>
</PropertyGroup>
```

### Example: Disabling in Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>

    <!-- Disable HTTP Client Factory Extension Generator -->
    <EnableMoqAutoMockerHttpClientFactoryGenerator>false</EnableMoqAutoMockerHttpClientFactoryGenerator>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Moq.AutoMock" Version="4.0.1" />
  </ItemGroup>
</Project>
```

## Troubleshooting

### Extension Method Not Available

1. Verify your project references `Microsoft.Extensions.Http`
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you're using the `Moq.AutoMock` namespace

### IHttpClientFactory Not Resolving

1. Make sure you call `WithHttpClientFactory()` before calling `CreateInstance<T>()`
2. Verify the service under test accepts `IHttpClientFactory` (not a concrete `HttpClient`)

## Best Practices

- Call `WithHttpClientFactory()` early in your test setup, before creating instances
- Use `SetupHttpGet`/`SetupHttpPost`/etc. as usual — they apply to all named clients since they share the same testable handler
- Combine with `HttpClientResolver`'s direct `HttpClient` support when a class depends on both a concrete `HttpClient` and `IHttpClientFactory`

## See Also

- [AutoMocker API Reference](../Moq.AutoMock/AutoMocker.md)
- [Source Generators Overview](../SourceGenerators.md)
- [HttpClient Support](../HttpClient.md)
- [Moq.AutoMock on NuGet](https://www.nuget.org/packages/Moq.AutoMock)
