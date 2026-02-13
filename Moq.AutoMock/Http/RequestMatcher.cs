using System.Net.Http;

namespace Moq.AutoMock.Http;

/// <summary>
/// Custom Moq matchers for <see cref="HttpRequestMessage" /> using <see cref="Match.Create{T}(Predicate{T})" />.
/// </summary>
public static class RequestMatcher
{
    /**
     * The methods here are used to automatically generate the RequestExtensions.
     * Summaries must start with "A request matching", and parameters must be on a single line.
     * Verify any changes in the generated code.
     */

    /// <summary>
    /// A request matching the given predicate.
    /// </summary>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(Predicate<HttpRequestMessage> match)
    {
        if (match is null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        var requestPredicate = new RequestPredicate(match);

        return Match.Create(requestPredicate.Matches, () => Is(match));
    }

    /// <summary>
    /// A request matching the given predicate.
    /// </summary>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(Func<HttpRequestMessage, Task<bool>> match)
    {
        if (match is null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        var requestPredicate = new RequestPredicate(match);

        return Match.Create(requestPredicate.Matches, () => Is(match)); // Blocking
    }

    /// <summary>
    /// A request matching the given <see cref="Uri" />.
    /// </summary>
    /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    public static HttpRequestMessage Is(Uri requestUri)
        => Match.Create(r => r.RequestUri == requestUri, () => Is(requestUri));

    /// <summary>
    /// A request matching the given URL.
    /// </summary>
    /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    public static HttpRequestMessage Is(string requestUrl)
        => Is(new Uri(requestUrl));

    /// <summary>
    /// A request matching the given <see cref="Uri" /> as well as a predicate.
    /// </summary>
    /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(Uri requestUri, Predicate<HttpRequestMessage> match)
    {
        if (match is null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        var requestPredicate = new RequestPredicate(match);

        return Match.Create(
            r => r.RequestUri == requestUri && requestPredicate.Matches(r),
            () => Is(requestUri, match));
    }

    /// <summary>
    /// A request matching the given <see cref="Uri" /> as well as a predicate.
    /// </summary>
    /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
    {
        if (match is null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        var requestPredicate = new RequestPredicate(match);

        return Match.Create(
            r => r.RequestUri == requestUri && requestPredicate.Matches(r), // Blocking
            () => Is(requestUri, match));
    }

    /// <summary>
    /// A request matching the given URL as well as a predicate.
    /// </summary>
    /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(string requestUrl, Predicate<HttpRequestMessage> match)
        => Is(new Uri(requestUrl), match);

    /// <summary>
    /// A request matching the given URL as well as a predicate.
    /// </summary>
    /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
        => Is(new Uri(requestUrl), match);

    /// <summary>
    /// A request matching the given method as well as a predicate.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(HttpMethod method, Predicate<HttpRequestMessage> match)
    {
        if (match is null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        var requestPredicate = new RequestPredicate(match);

        return Match.Create(
            r => r.Method == method && requestPredicate.Matches(r),
            () => Is(method, match));
    }

    /// <summary>
    /// A request matching the given method as well as a predicate.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(HttpMethod method, Func<HttpRequestMessage, Task<bool>> match)
    {
        if (match is null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        var requestPredicate = new RequestPredicate(match);

        return Match.Create(
            r => r.Method == method && requestPredicate.Matches(r), // Blocking
            () => Is(method, match));
    }

    /// <summary>
    /// A request matching the given method and <see cref="Uri" />.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
    /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    public static HttpRequestMessage Is(HttpMethod method, Uri requestUri)
        => Match.Create(
            r => r.Method == method && r.RequestUri == requestUri,
            () => Is(method, requestUri));

    /// <summary>
    /// A request matching the given method and URL.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
    /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    public static HttpRequestMessage Is(HttpMethod method, string requestUrl)
        => Is(method, new Uri(requestUrl));

    /// <summary>
    /// A request matching the given method and <see cref="Uri" /> as well as a predicate.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
    /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(HttpMethod method, Uri requestUri, Predicate<HttpRequestMessage> match)
    {
        if (match is null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        var requestPredicate = new RequestPredicate(match);

        return Match.Create(
            r => r.Method == method && r.RequestUri == requestUri && requestPredicate.Matches(r),
            () => Is(method, requestUri, match));
    }

    /// <summary>
    /// A request matching the given method and <see cref="Uri" /> as well as a predicate.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
    /// <param name="requestUri">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(HttpMethod method, Uri requestUri, Func<HttpRequestMessage, Task<bool>> match)
    {
        if (match is null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        var requestPredicate = new RequestPredicate(match);

        return Match.Create(
            r => r.Method == method && r.RequestUri == requestUri && requestPredicate.Matches(r), // Blocking
            () => Is(method, requestUri, match));
    }

    /// <summary>
    /// A request matching the given method and URL as well as a predicate.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
    /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(HttpMethod method, string requestUrl, Predicate<HttpRequestMessage> match)
        => Is(method, new Uri(requestUrl), match);

    /// <summary>
    /// A request matching the given method and URL as well as a predicate.
    /// </summary>
    /// <param name="method">The <see cref="HttpRequestMessage.Method" />.</param>
    /// <param name="requestUrl">The <see cref="HttpRequestMessage.RequestUri" />.</param>
    /// <param name="match">The predicate used to match the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
    public static HttpRequestMessage Is(HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
        => Is(method, new Uri(requestUrl), match);
}
