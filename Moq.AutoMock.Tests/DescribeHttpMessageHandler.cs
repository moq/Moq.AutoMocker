using System.Net;
using System.Net.Http;
using Moq.AutoMock.Http;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeHttpMessageHandler
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task HttpClient_CanSetupResponses()
    {
        var mocker = new AutoMocker();

        // Setup the mock handler to return a specific response using Protected() API
        mocker.GetMock<HttpMessageHandler>()
            .SetupHttp(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsHttpResponse(HttpStatusCode.OK, "Hello, World!");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);
        Assert.AreEqual("Hello, World!", content);
    }

    [TestMethod]
    public async Task HttpClient_WithStrickMocks_ThrowsWithoutSetup()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);

        HttpClient httpClient = mocker.GetMock<HttpMessageHandler>().CreateClient();

        await Assert.ThrowsAsync<MockException>(() => httpClient.GetAsync("https://example.com/api/test", TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task HttpClient_CanVerifySpecificRequestsWereMade()
    {
        var mocker = new AutoMocker();

        mocker.GetMock<HttpMessageHandler>()
            .SetupHttp(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsHttpResponse(HttpStatusCode.OK);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await service.GetAsync("https://example.com/api/users");
        await service.PostAsync("https://example.com/api/users", "{}");

        mocker.GetMock<HttpMessageHandler>()
            .Verify(x => x.SendAsync(
                It.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                It.IsAny<CancellationToken>()), Times.Once());

        mocker.GetMock<HttpMessageHandler>()
            .Verify(x => x.SendAsync(
                It.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                It.IsAny<CancellationToken>()), Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupSequenceOfResponses()
    {
        var mocker = new AutoMocker();

        mocker.GetMock<HttpMessageHandler>()
            .SetupSequence(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsHttpResponse(HttpStatusCode.ServiceUnavailable)
            .ReturnsHttpResponse(HttpStatusCode.OK, "Success");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var firstResponse = await service.GetAsync("https://example.com/api/test");
        var secondResponse = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, firstResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, secondResponse.StatusCode);
    }
}
