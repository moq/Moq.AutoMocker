using System.Net;
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
            messageHandler.DefaultValueProvider = HttpMessageHandlerDefaultValueProvider.Instance;
            context.Value = messageHandler.CreateClient();
        }
    }

    /// <summary>
    /// The testable HTTP Client handler. It will return empty 200 responsees to all requests.
    /// </summary>
    private class HttpMessageHandlerDefaultValueProvider : DefaultValueProvider
    {
        public static HttpMessageHandlerDefaultValueProvider Instance { get; } = new();

        protected override object GetDefaultValue(Type type, Mock mock)
        {
            if (type == typeof(Task<HttpResponseMessage>))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(string.Empty)
                });
            }

            throw new InvalidOperationException($"Unknown return type '{type.FullName}' for default value on mock '{mock.Object.GetType().FullName}'");
        }
    }
}
