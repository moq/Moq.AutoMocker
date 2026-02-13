using System.Net.Http;
using System.Runtime.CompilerServices;

namespace Moq.AutoMock.Http;

/// <summary>
/// Memoizes the result of a `match` predicate for a given request to prevent the predicate from being called
/// repeatedly, which may cause it to throw if the request content had been consumed and its stream closed.
/// </summary>
internal class RequestPredicate(Func<HttpRequestMessage, Task<bool>> match)
{
    private readonly Func<HttpRequestMessage, Task<bool>> _match = match ?? throw new ArgumentNullException(nameof(match));
    private readonly ConditionalWeakTable<HttpRequestMessage, Task<bool>> _memoizedResults = new();

    public RequestPredicate(Predicate<HttpRequestMessage> match)
        : this(r => Task.FromResult(match(r)))
    { }

    public Task<bool> MatchesAsync(HttpRequestMessage request)
    {
        if (!_memoizedResults.TryGetValue(request, out var result))
        {
            result = _match(request);
            _memoizedResults.Add(request, result);
        }

        return result;
    }

    public bool Matches(HttpRequestMessage request) => MatchesAsync(request).Result;
}
