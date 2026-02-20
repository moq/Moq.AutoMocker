using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Moq.AutoMock.Http;
using Moq.Language;
using Moq.Language.Flow;

namespace Moq.AutoMock;

/// <summary>
/// Provides extension methods for configuring mocked HTTP responses in unit tests using Moq and HttpMessageHandler.
/// These methods enable fluent setup of various response types, including plain text, JSON, byte arrays, and streams,
/// for both single and sequential HTTP requests.
/// </summary>
/// <remarks>These extension methods are intended to simplify the setup of mocked HTTP responses when testing code
/// that relies on HttpClient. They support a range of content types and allow for custom configuration of response
/// headers and status codes. When using stream-based responses, seekable streams are wrapped to ensure each request
/// receives an independent stream position, enabling reuse across multiple requests. All methods are designed for use
/// with Moq's ISetup and ISetupSequentialResult interfaces targeting HttpMessageHandler.</remarks>
public static partial class MockHttpMessageHandlerExtensions
{
    private static HttpResponseMessage CreateResponse(
        HttpRequestMessage? request = null,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        HttpContent? content = null,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            RequestMessage = request,
            Content = content
        };

        if (content != null && mediaType != null)
        {
            content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        }

        configure?.Invoke(response);
        return response;
    }

    /// <summary>
    /// Specifies the response to return.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    public static IReturnsResult<HttpMessageHandler> ReturnsHttpResponse(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        Action<HttpResponseMessage>? configure = null)
    {
        return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) 
            => CreateResponse(
                request: request,
                statusCode: statusCode,
                configure: configure));
    }

    /// <summary>
    /// Specifies the response to return in sequence.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsHttpResponse(
        this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        Action<HttpResponseMessage>? configure = null)
    {
        return setup.ReturnsAsync(CreateResponse(
            statusCode: statusCode,
            configure: configure));
    }

    /// <summary>
    /// Specifies the response to return.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The response content.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static IReturnsResult<HttpMessageHandler> ReturnsHttpResponse(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        HttpContent content,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) 
            => CreateResponse(
                request: request,
                statusCode: statusCode,
                content: content,
                configure: configure)
            );
    }

    /// <summary>
    /// Specifies the response to return in sequence.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The response content.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsHttpResponse(
        this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        HttpContent content,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync(CreateResponse(
            statusCode: statusCode,
            content: content,
            configure: configure));
    }

    /// <summary>
    /// Specifies the response to return, as <see cref="StringContent" />.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type. Defaults to text/plain.</param>
    /// <param name="encoding">The character encoding. Defaults to <see cref="Encoding.UTF8" />.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static IReturnsResult<HttpMessageHandler> ReturnsHttpResponse(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        string content,
        string? mediaType = null,
        Encoding? encoding = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) 
            => CreateResponse(
                request: request,
                statusCode: statusCode,
                content: new StringContent(content, encoding, mediaType ?? "text/plain"),
                configure: configure)
            );
    }

    /// <summary>
    /// Specifies the response to return in sequence, as <see cref="StringContent" />.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type. Defaults to text/plain.</param>
    /// <param name="encoding">The character encoding. Defaults to <see cref="Encoding.UTF8" />.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsHttpResponse(
        this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        string content,
        string? mediaType = null,
        Encoding? encoding = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync(CreateResponse(
            statusCode: statusCode,
            content: new StringContent(content, encoding, mediaType ?? "text/plain"),
            configure: configure));
    }

    /// <summary>
    /// Specifies the response to return, as <see cref="StringContent" /> with <see cref="HttpStatusCode.OK" />.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type. Defaults to text/plain.</param>
    /// <param name="encoding">The character encoding. Defaults to <see cref="Encoding.UTF8" />.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static IReturnsResult<HttpMessageHandler> ReturnsHttpResponse(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
        string content,
        string? mediaType = null,
        Encoding? encoding = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
        {
            return CreateResponse(
                request: request,
                content: new StringContent(content, encoding, mediaType ?? "text/plain"),
                configure: configure);
        });
    }

    /// <summary>
    /// Specifies the response to return in sequence, as <see cref="StringContent" /> with <see
    /// cref="HttpStatusCode.OK" />.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type. Defaults to text/plain.</param>
    /// <param name="encoding">The character encoding. Defaults to <see cref="Encoding.UTF8" />.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
        this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
        string content,
        string? mediaType = null,
        Encoding? encoding = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync(CreateResponse(
            content: new StringContent(content, encoding, mediaType ?? "text/plain"),
            configure: configure));
    }

    // <summary>
    // Specifies the response to return, as <see cref="JsonContent" /> using System.Text.Json.
    // </summary>
    // <param name="setup">The setup.</param>
    // <param name="statusCode">The status code.</param>
    // <param name="value">The value to serialize.</param>
    // <param name="options">Options to control the behavior during serialization, the default options are <see
    // cref="JsonSerializerDefaults.Web" />.</param>
    // <param name="configure">An action to configure the response headers.</param>
    //public static IReturnsResult<HttpMessageHandler> ReturnsJsonResponse<T>(
    //    this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
    //    HttpStatusCode statusCode,
    //    T value,
    //    JsonSerializerOptions? options = null,
    //    Action<HttpResponseMessage>? configure = null)
    //{
    //    return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
    //    {
    //        return CreateResponse(
    //            request: request,
    //            statusCode: statusCode,
    //            content: JsonContent.Create(value, options: options),
    //            configure: configure);
    //    });
    //}

    // <summary>
    // Specifies the response to return in sequence, as <see cref="JsonContent" /> using System.Text.Json.
    // </summary>
    // <param name="setup">The setup.</param>
    // <param name="statusCode">The status code.</param>
    // <param name="value">The value to serialize.</param>
    // <param name="options">Options to control the behavior during serialization, the default options are <see
    // cref="JsonSerializerDefaults.Web" />.</param>
    // <param name="configure">An action to configure the response headers.</param>
    //public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJsonResponse<T>(
    //    this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
    //    HttpStatusCode statusCode,
    //    T value,
    //    JsonSerializerOptions? options = null,
    //    Action<HttpResponseMessage> configure = null)
    //{
    //    return setup.ReturnsAsync(CreateResponse(
    //        statusCode: statusCode,
    //        content: JsonContent.Create(value, options: options),
    //        configure: configure));
    //}

    // <summary>
    // Specifies the response to return, as <see cref="JsonContent" /> using System.Text.Json with <see cref="HttpStatusCode.OK" />.
    // </summary>
    // <param name="setup">The setup.</param>
    // <param name="value">The value to serialize.</param>
    // <param name="options">Options to control the behavior during serialization, the default options are <see
    // cref="JsonSerializerDefaults.Web" />.</param>
    // <param name="configure">An action to configure the response headers.</param>
    //public static IReturnsResult<HttpMessageHandler> ReturnsJsonResponse<T>(
    //    this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
    //    T value,
    //    JsonSerializerOptions? options = null,
    //    Action<HttpResponseMessage> configure = null)
    //{
    //    return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
    //    {
    //        return CreateResponse(
    //            request: request,
    //            content: JsonContent.Create(value, options: options),
    //            configure: configure);
    //    });
    //}

    // <summary>
    // Specifies the response to return in sequence, as <see cref="JsonContent" /> using System.Text.Json with <see
    // cref="HttpStatusCode.OK" />.
    // </summary>
    // <param name="setup">The setup.</param>
    // <param name="value">The value to serialize.</param>
    // <param name="options">Options to control the behavior during serialization, the default options are <see
    // cref="JsonSerializerDefaults.Web" />.</param>
    // <param name="configure">An action to configure the response headers.</param>
    //public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJsonResponse<T>(
    //    this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
    //    T value,
    //    JsonSerializerOptions? options = null,
    //    Action<HttpResponseMessage>? configure = null)
    //{
    //    return setup.ReturnsAsync(CreateResponse(
    //        content: JsonContent.Create(value, options: options),
    //        configure: configure));
    //}

    /// <summary>
    /// Specifies the response to return, as <see cref="ByteArrayContent" />.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        byte[] content,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) =>
        {
            return CreateResponse(
                request: request,
                statusCode: statusCode,
                content: new ByteArrayContent(content),
                mediaType: mediaType,
                configure: configure);
        });
    }

    /// <summary>
    /// Specifies the response to return in sequence, as <see cref="ByteArrayContent" />.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
        this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        byte[] content,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync(CreateResponse(
            statusCode: statusCode,
            content: new ByteArrayContent(content),
            mediaType: mediaType,
            configure: configure));
    }

    /// <summary>
    /// Specifies the response to return, as <see cref="ByteArrayContent" /> with <see cref="HttpStatusCode.OK" />.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
        byte[] content,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) 
            => CreateResponse(
                request: request,
                content: new ByteArrayContent(content),
                mediaType: mediaType,
                configure: configure)
            );
    }

    /// <summary>
    /// Specifies the response to return in sequence, as <see cref="ByteArrayContent" /> with <see
    /// cref="HttpStatusCode.OK" />.
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
        this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
        byte[] content,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync(CreateResponse(
            content: new ByteArrayContent(content),
            mediaType: mediaType,
            configure: configure));
    }

    /// <summary>
    /// Specifies the response to return, as <see cref="StreamContent" />. If the stream is seekable, it will be
    /// wrapped to allow for reuse across multiple requests (each request maintains an independent stream position).
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        Stream content,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) 
            => CreateResponse(
                request: request,
                statusCode: statusCode,
                content: CreateStreamContent(content),
                mediaType: mediaType,
                configure: configure)
            );
    }

    /// <summary>
    /// Specifies the response to return in sequence, as <see cref="StreamContent" />. If the stream is seekable, it
    /// will be wrapped to allow for reuse across multiple requests (each request maintains an independent stream
    /// position).
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="statusCode">The status code.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
        this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
        HttpStatusCode statusCode,
        Stream content,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync(CreateResponse(
            statusCode: statusCode,
            content: CreateStreamContent(content),
            mediaType: mediaType,
            configure: configure));
    }

    /// <summary>
    /// Specifies the response to return, as <see cref="StreamContent" /> with <see cref="HttpStatusCode.OK" />. If
    /// the stream is seekable, it will be wrapped to allow for reuse across multiple requests (each request
    /// maintains an independent stream position).
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static IReturnsResult<HttpMessageHandler> ReturnsResponse(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup,
        Stream content,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync((HttpRequestMessage request, CancellationToken _) 
            => CreateResponse(
                request: request,
                content: CreateStreamContent(content),
                mediaType: mediaType,
                configure: configure)
            );
    }

    /// <summary>
    /// Specifies the response to return in sequence, as <see cref="StreamContent" /> with <see
    /// cref="HttpStatusCode.OK" />. If the stream is seekable, it will be wrapped to allow for reuse across
    /// multiple requests (each request maintains an independent stream position).
    /// </summary>
    /// <param name="setup">The setup.</param>
    /// <param name="content">The response body.</param>
    /// <param name="mediaType">The media type.</param>
    /// <param name="configure">An action to configure the response headers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="content" /> is null.</exception>
    public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsResponse(
        this ISetupSequentialResult<Task<HttpResponseMessage>> setup,
        Stream content,
        string? mediaType = null,
        Action<HttpResponseMessage>? configure = null)
    {
        if (content is null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return setup.ReturnsAsync(CreateResponse(
            content: CreateStreamContent(content),
            mediaType: mediaType,
            configure: configure));
    }

    private static StreamContent CreateStreamContent(Stream content) =>
        new(content.CanSeek ? new ResponseStream(content) : content);
}
