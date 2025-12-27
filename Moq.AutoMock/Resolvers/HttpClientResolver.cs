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

            if (context.AutoMocker.MockBehavior == MockBehavior.Loose)
            {
                messageHandler.SetupHttp(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            }
            context.Value = messageHandler.CreateClient();
        }
    }
}
