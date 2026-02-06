using System.Net;
using System.Net.Http;
using Moq.AutoMock.Http;
using Moq.Protected;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeHttpClient
{
    [TestMethod]
    public void HttpClient_IsAutomaticallyResolved()
    {
        var mocker = new AutoMocker();

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        Assert.IsNotNull(service.HttpClient);
    }

    [TestMethod]
    public void HttpClient_UsesTheSameMockedHandler()
    {
        var mocker = new AutoMocker();

        var service1 = mocker.CreateInstance<ServiceWithHttpClient>();
        var service2 = mocker.CreateInstance<ServiceWithHttpClient>();

        // Both services should use clients backed by the same handler mock
        Assert.AreSame(service1.HttpClient, service2.HttpClient);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupResponsesViaHandler()
    {
        var mocker = new AutoMocker();

        // Setup the mock handler to return a specific response using Protected() API
        mocker.GetMock<HttpMessageHandler>()
            .SetupHttp(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsResponse(HttpStatusCode.OK, "Hello, World!");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);
        Assert.AreEqual("Hello, World!", content);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupDifferentResponsesForDifferentUrls()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpGet("/users")
            .ReturnsResponse(HttpStatusCode.OK, """{"users": []}""");

        mocker.SetupHttpGet("/products")
            .ReturnsResponse(HttpStatusCode.OK, """{"products": []}""");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var usersResponse = await service.GetAsync("https://example.com/api/users");
        var productsResponse = await service.GetAsync("https://example.com/api/products");

        var usersContent = await usersResponse.Content.ReadAsStringAsync(TestContext.CancellationToken);
        var productsContent = await productsResponse.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual("""{"users": []}""", usersContent);
        Assert.AreEqual("""{"products": []}""", productsContent);
    }

    [TestMethod]
    public async Task HttpClient_CanVerifyRequestsWereMade()
    {
        var mocker = new AutoMocker();

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await service.GetAsync("https://example.com/api/test");

        mocker.VerifyHttpGet("https://example.com/api/test", Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_WithStrickMocks_ThrowsWithoutSetup()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await Assert.ThrowsAsync<MockException>(() => service.GetAsync("https://example.com/api/test"));
    }

    [TestMethod]
    public async Task HttpClient_CanVerifySpecificRequestsWereMade()
    {
        var mocker = new AutoMocker();

        mocker.GetMock<HttpMessageHandler>()
            .SetupHttp(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsResponse(HttpStatusCode.OK);

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
            .ReturnsResponse(HttpStatusCode.ServiceUnavailable)
            .ReturnsResponse(HttpStatusCode.OK, "Success");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var firstResponse = await service.GetAsync("https://example.com/api/test");
        var secondResponse = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, firstResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, secondResponse.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupErrorResponses()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpGet()
            .ReturnsResponse(HttpStatusCode.InternalServerError, "Server Error");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupNotFoundResponse()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpGet()
            .ReturnsResponse(HttpStatusCode.NotFound);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/api/nonexistent");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupByteArrayResponse()
    {
        var mocker = new AutoMocker();
        var expectedBytes = "Hello"u8.ToArray();

        mocker.SetupHttpGet()
            .ReturnsResponse(HttpStatusCode.OK, expectedBytes, "application/octet-stream");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/api/binary");
        var content = await response.Content.ReadAsByteArrayAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        CollectionAssert.AreEqual(expectedBytes, content);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupResponseWithCustomHeaders()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpGet()
            .ReturnsResponse(HttpStatusCode.OK, "Response with headers", configure: response =>
            {
                response.Headers.Add("X-Custom-Header", "CustomValue");
            });

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(response.Headers.Contains("X-Custom-Header"));
        Assert.AreEqual("CustomValue", response.Headers.GetValues("X-Custom-Header").First());
    }

    [TestMethod]
    public void CreateClient_ReturnsHttpClientBackedByMockedHandler()
    {
        var mocker = new AutoMocker();
        var handlerMock = mocker.GetMock<HttpMessageHandler>();

        // Use the extension method to create a client
        var client = handlerMock.CreateClient();

        Assert.IsNotNull(client);
    }

    private class ServiceWithHttpClient(HttpClient httpClient)
    {
        public HttpClient HttpClient => httpClient;

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            return httpClient.GetAsync(url);
        }

        public Task<HttpResponseMessage> PostAsync(string url, string content)
        {
            return httpClient.PostAsync(url, new StringContent(content));
        }
    }

    public TestContext TestContext { get; set; }
}
