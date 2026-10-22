# HTTP

Extensions: `Moq.AutoMock.MockHttpMessageHandlerExtensions`. Handler contract and matchers: `Moq.AutoMock.Http.IHttpMessageHandler`, `Moq.AutoMock.Http.RequestMatcher`. [Documentation index](README.md).

`AutoMocker` resolves `HttpClient` with a mocked `HttpMessageHandler`. Unconfigured requests return HTTP 200 with empty content in loose mode; strict mode requires explicit setups.

```csharp
using System.Net;
using System.Net.Http;
using Moq;
using Moq.AutoMock;
using Moq.AutoMock.Http;

var mocker = new AutoMocker();
mocker.SetupHttpGet("/users")
    .ReturnsHttpResponse(HttpStatusCode.OK, "[]", mediaType: "application/json");

using var response = await mocker.Get<HttpClient>()
    .GetAsync("https://example.test/users");
mocker.VerifyHttpGet("/users", Times.Once());
```

## Verb-specific setup

Each method below has overloads on **both** `AutoMocker` and `Mock<HttpMessageHandler>`. All return `ISetup<HttpMessageHandler, Task<HttpResponseMessage>>`.

Receivers are omitted:

| Method | Overloads |
| --- | --- |
| `SetupHttpGet` | `(string? requestUri = null)`; `(Expression<Func<HttpRequestMessage, bool>> match)` |
| `SetupHttpPost` | `(string? requestUri = null, string? content = null)`; `(Expression<Func<HttpRequestMessage, bool>> match)` |
| `SetupHttpPut` | `(string? requestUri = null, string? content = null)`; `(Expression<Func<HttpRequestMessage, bool>> match)` |
| `SetupHttpPatch` | `(string? requestUri = null, string? content = null)`; `(Expression<Func<HttpRequestMessage, bool>> match)` |
| `SetupHttpDelete` | `(string? requestUri = null)`; `(Expression<Func<HttpRequestMessage, bool>> match)` |
| `SetupHttpHead` | `(string? requestUri = null)`; `(Expression<Func<HttpRequestMessage, bool>> match)` |

Verb helpers restrict the HTTP method. URI matching is a case-sensitive substring of the URI text; body matching is exact text. Null URI/content filters match anything.

```csharp
mocker.SetupHttpPost("/orders", "order data")
    .ReturnsHttpResponse(HttpStatusCode.Created);
mocker.SetupHttpPut("/orders/1", "replacement")
    .ReturnsHttpResponse(HttpStatusCode.OK);
mocker.SetupHttpPatch("/orders/1", "change")
    .ReturnsHttpResponse(HttpStatusCode.OK);
mocker.SetupHttpDelete("/orders/1")
    .ReturnsHttpResponse(HttpStatusCode.NoContent);
mocker.SetupHttpHead("/health")
    .ReturnsHttpResponse(HttpStatusCode.OK);

mocker.GetMock<HttpMessageHandler>()
    .SetupHttpGet(r => r.RequestUri!.AbsolutePath == "/users")
    .ReturnsHttpResponse("[]");
```

## SetupHttp, SetupSequence, SetupHttpSequence, and conditional Setup

```csharp
ISetup<HttpMessageHandler, TResult> SetupHttp<TResult>(
    this Mock<HttpMessageHandler> handler,
    Expression<Func<IHttpMessageHandler, TResult>> expression)
ISetupSequentialResult<TResult> SetupSequence<TResult>(
    this Mock<HttpMessageHandler> handler,
    Expression<Func<IHttpMessageHandler, TResult>> expression)
ISetupSequentialResult<TResult> SetupHttpSequence<TResult>(
    this AutoMocker mocker,
    Expression<Func<IHttpMessageHandler, TResult>> expression)
ISetup<HttpMessageHandler, Task<HttpResponseMessage>> Setup(
    this ISetupConditionResult<HttpMessageHandler> handler,
    Expression<Func<IHttpMessageHandler, Task<HttpResponseMessage>>> expression)
```

`IHttpMessageHandler` exposes `Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)` as a public analogue of the handler's protected method. Use it in expressions; it is not a separately registered service.

```csharp
var handler = mocker.GetMock<HttpMessageHandler>();
handler.SetupHttp(x => x.SendAsync(
        It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
    .ReturnsHttpResponse(HttpStatusCode.OK);

mocker.SetupHttpSequence(x => x.SendAsync(
        It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
    .ReturnsHttpResponse(HttpStatusCode.ServiceUnavailable)
    .ReturnsResponse("recovered");
```

`SetupSequence` returns successive responses for one setup. Conditional `Setup` instead supports `When(...)` or Moq's `InSequence(...)`:

```csharp
bool enabled = true;
handler.When(() => enabled)
    .Setup(x => x.SendAsync(
        It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
    .ReturnsHttpResponse(HttpStatusCode.OK);
```

Conditional expressions must directly call `SendAsync` on their `IHttpMessageHandler` parameter; invalid expression shapes throw `ArgumentException`.

## RequestMatcher.Is overloads

These static matchers return `HttpRequestMessage` for use **inside Moq setup/verification expressions**, not for sending requests. URI matching uses equality, unlike the substring filters on verb helpers. String URL overloads create a `Uri` and therefore require a valid absolute URL.

```csharp
HttpRequestMessage Is(Predicate<HttpRequestMessage> match)
HttpRequestMessage Is(Func<HttpRequestMessage, Task<bool>> match)
HttpRequestMessage Is(Uri requestUri)
HttpRequestMessage Is(string requestUrl)
HttpRequestMessage Is(Uri requestUri, Predicate<HttpRequestMessage> match)
HttpRequestMessage Is(Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
HttpRequestMessage Is(string requestUrl, Predicate<HttpRequestMessage> match)
HttpRequestMessage Is(string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
HttpRequestMessage Is(HttpMethod method, Predicate<HttpRequestMessage> match)
HttpRequestMessage Is(HttpMethod method, Func<HttpRequestMessage, Task<bool>> match)
HttpRequestMessage Is(HttpMethod method, Uri requestUri)
HttpRequestMessage Is(HttpMethod method, string requestUrl)
HttpRequestMessage Is(
    HttpMethod method, Uri requestUri, Predicate<HttpRequestMessage> match)
HttpRequestMessage Is(
    HttpMethod method, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
HttpRequestMessage Is(
    HttpMethod method, string requestUrl, Predicate<HttpRequestMessage> match)
HttpRequestMessage Is(
    HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
```

Predicates are memoized per request so content need not be read repeatedly. Asynchronous predicates are evaluated synchronously by Moq's matching pipeline; avoid callbacks that depend on a captured synchronization context. Null predicates throw `ArgumentNullException`.

```csharp
handler.SetupHttp(x => x.SendAsync(
        RequestMatcher.Is(HttpMethod.Get, "https://example.test/users"),
        It.IsAny<CancellationToken>()))
    .ReturnsHttpResponse("[]");

Func<HttpRequestMessage, Task<bool>> matchesContent = async request =>
    await request.Content!.ReadAsStringAsync().ConfigureAwait(false) == "order data";
handler.SetupHttp(x => x.SendAsync(
        RequestMatcher.Is(HttpMethod.Post, "https://example.test/orders", matchesContent),
        It.IsAny<CancellationToken>()))
    .ReturnsHttpResponse(HttpStatusCode.Created);
```

## Response builders

For ordinary setups, the receiver is `ISetup<HttpMessageHandler, Task<HttpResponseMessage>>` and the return type is `IReturnsResult<HttpMessageHandler>`. Sequential variants receive and return `ISetupSequentialResult<Task<HttpResponseMessage>>`.

Receivers and return types are omitted. `configure` changes headers or other response properties.

### ReturnsHttpResponse

```csharp
ReturnsHttpResponse(
    HttpStatusCode statusCode, Action<HttpResponseMessage>? configure = null)
ReturnsHttpResponse(
    HttpStatusCode statusCode, HttpContent content,
    Action<HttpResponseMessage>? configure = null)
ReturnsHttpResponse(
    HttpStatusCode statusCode, string content,
    string? mediaType = null, Encoding? encoding = null,
    Action<HttpResponseMessage>? configure = null)
ReturnsHttpResponse(
    string content, string? mediaType = null, Encoding? encoding = null,
    Action<HttpResponseMessage>? configure = null)
```

The first three signatures support both receivers. The string-only `ReturnsHttpResponse` supports **ordinary setups only**; its sequential counterpart is named **`ReturnsResponse(string, ...)`**.

String content defaults to UTF-8 and `text/plain`; status-free overloads use HTTP 200.

```csharp
mocker.SetupHttpGet("/users")
    .ReturnsHttpResponse(HttpStatusCode.OK, "[]", "application/json",
        configure: response => response.Headers.Add("X-Test", "true"));
mocker.SetupHttpGet("/custom")
    .ReturnsHttpResponse(HttpStatusCode.OK, new StringContent("custom"));
```

### ReturnsResponse

```csharp
ReturnsResponse(
    HttpStatusCode statusCode, byte[] content, string? mediaType = null,
    Action<HttpResponseMessage>? configure = null)
ReturnsResponse(
    byte[] content, string? mediaType = null,
    Action<HttpResponseMessage>? configure = null)
ReturnsResponse(
    HttpStatusCode statusCode, Stream content, string? mediaType = null,
    Action<HttpResponseMessage>? configure = null)
ReturnsResponse(
    Stream content, string? mediaType = null,
    Action<HttpResponseMessage>? configure = null)
// Sequential receiver only:
ReturnsResponse(
    string content, string? mediaType = null, Encoding? encoding = null,
    Action<HttpResponseMessage>? configure = null)
```

Byte and stream overloads support both receivers. Status-free overloads use HTTP 200. Ordinary setups create a response per call; supplied `HttpContent` is still shared. Seekable streams are wrapped with independent positions. Sequential builders pre-create each response. All content overloads require non-null content.

```csharp
mocker.SetupHttpGet("/bytes")
    .ReturnsResponse(new byte[] { 1, 2, 3 }, "application/octet-stream");
mocker.SetupHttpGet("/stream")
    .ReturnsResponse(HttpStatusCode.OK, new MemoryStream(new byte[] { 4, 5 }));
```

## Verification

Verb-specific methods extend **both** `AutoMocker` and `Mock<HttpMessageHandler>`. Receivers are omitted:

```csharp
void VerifyHttpGet(string requestUri, Times? times = null, string? failMessage = null)
void VerifyHttpPost(
    string requestUri, string? content = null, Times? times = null, string? failMessage = null)
void VerifyHttpPut(
    string requestUri, string? content = null, Times? times = null, string? failMessage = null)
void VerifyHttpPatch(
    string requestUri, string? content = null, Times? times = null, string? failMessage = null)
void VerifyHttpDelete(string requestUri, Times? times = null, string? failMessage = null)
void VerifyHttpHead(string requestUri, Times? times = null, string? failMessage = null)
```

Lower-level methods extend **Mock&lt;HttpMessageHandler&gt; only**:

```csharp
void VerifyHttp(
    Expression<Func<HttpRequestMessage, bool>> match,
    Times? times = null, string? failMessage = null)
void VerifyHttp<TResult>(
    Expression<Func<IHttpMessageHandler, TResult>> expression,
    Times? times = null, string? failMessage = null)
void Verify<TResult>(
    Expression<Func<IHttpMessageHandler, TResult>> expression,
    Times? times = null, string? failMessage = null)
```

Matching follows setup rules. Omitted `times` means at least once. `failMessage` customizes the `MockException` raised on failure.

```csharp
mocker.VerifyHttpPost("/orders", "order data", Times.Once());
mocker.VerifyHttpPut("/orders/1", "replacement", Times.Once());
mocker.VerifyHttpPatch("/orders/1", "change", Times.Once());
mocker.VerifyHttpDelete("/orders/1", Times.Never());
mocker.VerifyHttpHead("/health", Times.Once());
handler.VerifyHttp(r => r.Method == HttpMethod.Get, Times.Once());
handler.Verify(x => x.SendAsync(
    It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()), Times.Once());
```

## CreateClient

```csharp
HttpClient CreateClient(this Mock<HttpMessageHandler> handler)
```

Creates a client without transferring handler ownership. Dispose the client separately. Unlike resolving `HttpClient` through the container, this helper alone does not configure fallback responses.

```csharp
using HttpClient client = mocker.GetMock<HttpMessageHandler>().CreateClient();
```

For named clients, see [WithHttpClientFactory](SourceGenerators/HttpClientFactoryExtensionGenerator.md). The generated factory caches one client per name; named clients share the testable handler.
