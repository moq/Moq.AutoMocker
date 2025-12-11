using System.Net.Http;
using Moq.AutoMock.Http;

namespace Moq.AutoMock.Resolvers;

/// <summary>
/// A resolver that can provide HttpClients with mocked HttpMessageHandler. 
/// </summary>
public class HttpClientResolver : IMockResolver
{
    /// <inheritdoc />
    public void Resolve(MockResolutionContext context)
    {
        if (context.RequestType == typeof(HttpClient))
        {
            var messageHandler = context.AutoMocker.GetMock<HttpMessageHandler>();
            context.Value = messageHandler.CreateClient();
        }
    }
}
