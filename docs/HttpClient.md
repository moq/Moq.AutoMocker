# HttpClient Support

Moq.AutoMocker provides built-in support for testing code that depends on `HttpClient`. When your class accepts an `HttpClient` constructor parameter, AutoMocker automatically resolves it with a mocked `HttpMessageHandler` — no manual setup required.

## Features

- **Automatic resolution** of `HttpClient` dependencies via `HttpClientResolver`
- **Verb-specific setup methods** for GET, POST, PUT, DELETE, and HEAD
- **Flexible request matching** by URI, expression predicate, or custom logic
- **Fluent response builders** for string, byte array, stream, and custom content types
- **Sequential responses** for testing retry logic and multi-call scenarios
- **Verification helpers** to assert that specific HTTP requests were made
- **Custom response configuration** including headers, status codes, and media types
- **Default behavior** returns HTTP 200 OK with empty content (loose mock)

## How It Works

AutoMocker registers an `HttpClientResolver` that intercepts `HttpClient` dependencies. When `CreateInstance<T>()` encounters an `HttpClient` parameter, the resolver:

1. Creates (or retrieves) a `Mock<HttpMessageHandler>`
2. Configures a default value provider that returns HTTP 200 OK responses
3. Wraps the handler in a new `HttpClient` instance

This means you can immediately create and test classes that use `HttpClient` without any explicit setup. The extension methods on `AutoMocker` and `Mock<HttpMessageHandler>` then let you customize request matching and response behavior.

## Usage

### Basic Setup

Create an `AutoMocker`, set up an HTTP response, and test your service:

```csharp
[TestMethod]
public async Task GetUsers_ReturnsUserList()
{
    // Arrange
    AutoMocker mocker = new();
    mocker.SetupHttpGet("/users")
        .ReturnsHttpResponse(HttpStatusCode.OK, """{"users": []}""");

    var service = mocker.CreateInstance<UserService>();

    // Act
    var response = await service.GetUsersAsync();

    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
}
```

Without any setup, requests return HTTP 200 OK with empty content by default.

### Setup by URL

Match requests by a URL substring. The match checks whether the request URI contains the specified string:

```csharp
mocker.SetupHttpGet("/users")
    .ReturnsHttpResponse(HttpStatusCode.OK, """{"users": []}""");

mocker.SetupHttpPost("/orders", "order data")
    .ReturnsHttpResponse(HttpStatusCode.Created, """{"id": 1}""");

mocker.SetupHttpPut("/users/1", "updated data")
    .ReturnsHttpResponse(HttpStatusCode.OK, """{"updated": true}""");

mocker.SetupHttpDelete("/users/1")
    .ReturnsHttpResponse(HttpStatusCode.NoContent);

mocker.SetupHttpHead("/health")
    .ReturnsHttpResponse(HttpStatusCode.OK);
```

### Setup by Expression

Use lambda expressions for more complex request matching:

```csharp
mocker.SetupHttpGet(r => r.RequestUri!.AbsoluteUri.EndsWith("/people"))
    .ReturnsHttpResponse(HttpStatusCode.OK, """[{"name": "Alice"}]""");

mocker.SetupHttpPost(r => r.RequestUri!.AbsoluteUri.Contains("/api/"))
    .ReturnsHttpResponse(HttpStatusCode.Accepted);
```

### Multiple URLs with Different Responses

```csharp
mocker.SetupHttpGet("/users")
    .ReturnsHttpResponse(HttpStatusCode.OK, """{"users": []}""");

mocker.SetupHttpGet("/products")
    .ReturnsHttpResponse(HttpStatusCode.OK, """{"products": []}""");

var service = mocker.CreateInstance<CatalogService>();

var usersResponse = await service.GetUsersAsync();    // matches /users
var productsResponse = await service.GetProductsAsync(); // matches /products
```

### Error Responses

```csharp
mocker.SetupHttpGet()
    .ReturnsHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

mocker.SetupHttpPost()
    .ReturnsHttpResponse(HttpStatusCode.BadRequest, """{"error": "Invalid input"}""");
```

### Custom Headers

Use the `configure` callback to modify response headers or other properties:

```csharp
mocker.SetupHttpGet()
    .ReturnsHttpResponse(HttpStatusCode.OK, "Response body", configure: response =>
    {
        response.Headers.Add("X-Custom-Header", "CustomValue");
        response.Headers.Add("X-Request-Id", "abc-123");
    });
```

### Binary Content

Return byte array responses with a specified media type:

```csharp
var pdfBytes = File.ReadAllBytes("sample.pdf");

mocker.SetupHttpGet("/documents/1")
    .ReturnsResponse(HttpStatusCode.OK, pdfBytes, "application/pdf");
```

### Stream Content

Return stream-based responses. Seekable streams are automatically wrapped so each request gets an independent stream position:

```csharp
using var stream = new MemoryStream(Encoding.UTF8.GetBytes("streamed content"));

mocker.SetupHttpGet("/download")
    .ReturnsResponse(HttpStatusCode.OK, stream, "application/octet-stream");
```

### Sequential Responses

Set up different responses for consecutive calls — useful for testing retry logic:

```csharp
mocker.SetupHttpSequence(x => x.SendAsync(
        It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
    .ReturnsHttpResponse(HttpStatusCode.ServiceUnavailable)
    .ReturnsHttpResponse(HttpStatusCode.OK, "Success");

var service = mocker.CreateInstance<ResilientService>();

var first = await service.CallApiAsync();  // 503 Service Unavailable
var second = await service.CallApiAsync(); // 200 OK
```

### Strict Mode

With `MockBehavior.Strict`, any request without an explicit setup throws a `MockException`:

```csharp
AutoMocker mocker = new(MockBehavior.Strict);
var service = mocker.CreateInstance<UserService>();

// Throws MockException — no setup for this request
await Assert.ThrowsAsync<MockException>(
    () => service.GetUsersAsync());
```

This is useful for ensuring your code only makes expected HTTP calls.

### Verb Isolation

Each setup method only matches its specific HTTP method. A `SetupHttpGet` will not match POST requests, and vice versa:

```csharp
AutoMocker mocker = new(MockBehavior.Strict);
mocker.SetupHttpGet("/people")
    .ReturnsHttpResponse(HttpStatusCode.OK, "[]");

var service = mocker.CreateInstance<PeopleService>();

// This POST throws because only GET is set up
await Assert.ThrowsAsync<MockException>(
    () => service.CreatePersonAsync("Alice"));
```

## Verification

Verify that specific HTTP requests were made during the test:

```csharp
var service = mocker.CreateInstance<NotificationService>();

await service.SendNotificationAsync("Hello");

// Verify a GET was made to a specific URL
mocker.VerifyHttpGet("https://example.com/api/status", Times.Once());

// Verify a POST with specific content
mocker.VerifyHttpPost("https://example.com/api/notify", "Hello", Times.Once());

// Verify PUT, DELETE, HEAD
mocker.VerifyHttpPut("https://example.com/api/config", "new value", Times.Once());
mocker.VerifyHttpDelete("https://example.com/api/cache", Times.Once());
mocker.VerifyHttpHead("https://example.com/api/health", Times.Once());
```

## Lower-Level Access

For advanced scenarios, access the `Mock<HttpMessageHandler>` directly:

```csharp
// Setup via the mock directly
mocker.GetMock<HttpMessageHandler>()
    .SetupHttp(x => x.SendAsync(
        It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
    .ReturnsHttpResponse(HttpStatusCode.OK, "Hello, World!");

// Create an HttpClient from the mock
HttpClient client = mocker.GetMock<HttpMessageHandler>().CreateClient();

// Verify via the mock directly
mocker.GetMock<HttpMessageHandler>()
    .Verify(x => x.SendAsync(
        It.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
        It.IsAny<CancellationToken>()), Times.Once());
```

## API Reference

### Setup Methods

All setup methods are available as extension methods on both `AutoMocker` and `Mock<HttpMessageHandler>`.

| Method | Description |
|--------|-------------|
| `SetupHttpGet(string?)` | Setup GET requests, optionally matching a URL substring |
| `SetupHttpGet(Expression)` | Setup GET requests matching an expression predicate |
| `SetupHttpPost(string?, string?)` | Setup POST requests with optional URL and content matching |
| `SetupHttpPost(Expression)` | Setup POST requests matching an expression predicate |
| `SetupHttpPut(string?, string?)` | Setup PUT requests with optional URL and content matching |
| `SetupHttpPut(Expression)` | Setup PUT requests matching an expression predicate |
| `SetupHttpDelete(string?)` | Setup DELETE requests, optionally matching a URL substring |
| `SetupHttpDelete(Expression)` | Setup DELETE requests matching an expression predicate |
| `SetupHttpHead(string?)` | Setup HEAD requests, optionally matching a URL substring |
| `SetupHttpHead(Expression)` | Setup HEAD requests matching an expression predicate |
| `SetupHttp<TResult>(Expression)` | Generic setup on `IHttpMessageHandler` (on `Mock<HttpMessageHandler>`) |
| `SetupHttpSequence<TResult>(Expression)` | Setup sequential responses (on `AutoMocker`) |
| `SetupSequence<TResult>(Expression)` | Setup sequential responses (on `Mock<HttpMessageHandler>`) |

### Response Methods

| Method | Description |
|--------|-------------|
| `ReturnsHttpResponse(HttpStatusCode)` | Return a response with the given status code |
| `ReturnsHttpResponse(HttpStatusCode, string)` | Return a string response with status code |
| `ReturnsHttpResponse(HttpStatusCode, HttpContent)` | Return custom content with status code |
| `ReturnsHttpResponse(string)` | Return a string response with HTTP 200 OK |
| `ReturnsResponse(HttpStatusCode, byte[], string?)` | Return byte array content with media type |
| `ReturnsResponse(HttpStatusCode, Stream, string?)` | Return stream content with media type |
| `ReturnsResponse(byte[], string?)` | Return byte array content with HTTP 200 OK |
| `ReturnsResponse(Stream, string?)` | Return stream content with HTTP 200 OK |

All response methods accept an optional `Action<HttpResponseMessage>? configure` parameter for customizing headers and other response properties. Sequential variants (`ISetupSequentialResult`) are also available for all response methods.

### Verification Methods

| Method | Target |
|--------|--------|
| `VerifyHttpGet(string, Times?, string?)` | Verify GET requests to a URL |
| `VerifyHttpPost(string, string?, Times?, string?)` | Verify POST requests with optional content |
| `VerifyHttpPut(string, string?, Times?, string?)` | Verify PUT requests with optional content |
| `VerifyHttpDelete(string, Times?, string?)` | Verify DELETE requests to a URL |
| `VerifyHttpHead(string, Times?, string?)` | Verify HEAD requests to a URL |
| `VerifyHttp(Expression, Times?, string?)` | Verify with a custom expression predicate |

### Utility Methods

| Method | Description |
|--------|-------------|
| `CreateClient()` | Create an `HttpClient` backed by a `Mock<HttpMessageHandler>` |

## Best Practices

- **Use verb-specific methods** (`SetupHttpGet`, `SetupHttpPost`, etc.) instead of the generic `SetupHttp` for clearer test intent and automatic HTTP method filtering
- **Match by URL substring** for simple cases (`"/users"`) and **expressions** for complex matching logic
- **Use strict mode** (`MockBehavior.Strict`) when you want to ensure no unexpected HTTP calls are made
- **Verify requests** to confirm your code sends the right HTTP method, URL, and content
- **Use sequential responses** to test retry patterns, circuit breakers, and degraded service scenarios
- **Leverage the `configure` callback** to test code that inspects response headers
