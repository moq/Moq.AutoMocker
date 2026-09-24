using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Http;
using Moq.AutoMock.Http;
using Moq.Protected;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeHttpClient
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void HttpClientFactory_ReturnsTheSameClientForTheSameName()
    {
        var mocker = new AutoMocker();
        mocker.WithHttpClientFactory();
        var factory = mocker.Get<IHttpClientFactory>();

        var first = factory.CreateClient("catalog");
        var second = factory.CreateClient("catalog");

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void HttpClientFactory_ReturnsDifferentClientsForDifferentNames()
    {
        var mocker = new AutoMocker();
        mocker.WithHttpClientFactory();
        var factory = mocker.Get<IHttpClientFactory>();

        var catalog = factory.CreateClient("catalog");
        var orders = factory.CreateClient("orders");

        Assert.AreNotSame(catalog, orders);
    }

    [TestMethod]
    public async Task HttpClientFactory_ReturnsTestableClients()
    {
        var mocker = new AutoMocker();
        mocker.WithHttpClientFactory();
        mocker.SetupHttpGet("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, "content");
        var client = mocker.Get<IHttpClientFactory>().CreateClient("catalog");

        var response = await client.GetAsync("https://example.com/people");

        Assert.AreEqual("content", await response.Content.ReadAsStringAsync(TestContext.CancellationToken));
        mocker.VerifyHttpGet("https://example.com/people", Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupDifferentResponsesForDifferentUrls()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpGet("/users")
            .ReturnsHttpResponse(HttpStatusCode.OK, """{"users": []}""");

        mocker.SetupHttpGet("/products")
            .ReturnsHttpResponse(HttpStatusCode.OK, """{"products": []}""");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var usersResponse = await service.GetAsync("https://example.com/api/users");
        var productsResponse = await service.GetAsync("https://example.com/api/products");

        var usersContent = await usersResponse.Content.ReadAsStringAsync(TestContext.CancellationToken);
        var productsContent = await productsResponse.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual("""{"users": []}""", usersContent);
        Assert.AreEqual("""{"products": []}""", productsContent);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupSequenceOfResponses()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpSequence(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsHttpResponse(HttpStatusCode.ServiceUnavailable)
            .ReturnsHttpResponse(HttpStatusCode.OK, "Success");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var firstResponse = await service.GetAsync("https://example.com/api/test");
        var secondResponse = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, firstResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, secondResponse.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_WithStrickMocks_ThrowsWithoutSetup()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await Assert.ThrowsAsync<MockException>(() => service.GetAsync("https://example.com/api/test"));
    }

    [TestMethod]
    public async Task HttpClient_CanVerifyHttpGetRequestsWereMade()
    {
        var mocker = new AutoMocker();

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await service.GetAsync("https://example.com/api/test");

        mocker.VerifyHttpGet("https://example.com/api/test", Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpGetByUrl()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpGet("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/people");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpGetByExpression()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpGet(r => r.RequestUri!.AbsoluteUri.EndsWith("/people"))
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/people");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_SetupHttpGetDoesNotMatchOtherVerbs()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);
        string content = """[{name: "test"}]""";
        mocker.SetupHttpGet("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);
        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await Assert.ThrowsAsync<MockException>(() => service.PostAsync("https://example.com/people", "data"));
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpGetErrorResponses()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpGet()
            .ReturnsHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpGetByteArrayResponse()
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
    public async Task HttpClient_CanSetupHttpGetResponseWithCustomHeaders()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpGet()
            .ReturnsHttpResponse(HttpStatusCode.OK, "Response with headers", configure: response => response.Headers.Add("X-Custom-Header", "CustomValue"));

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.GetAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(response.Headers.Contains("X-Custom-Header"));
        Assert.AreEqual("CustomValue", response.Headers.GetValues("X-Custom-Header").First());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPostByUrl()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpPost("/people", "data")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PostAsync("https://example.com/people", "data");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPostByExpression()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpPost(r => r.RequestUri!.AbsoluteUri.EndsWith("/people"))
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PostAsync("https://example.com/people", "data");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_SetupHttpPostDoesNotMatchOtherVerbs()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);
        string content = """[{name: "test"}]""";
        mocker.SetupHttpPost("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);
        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await Assert.ThrowsAsync<MockException>(() => service.GetAsync("https://example.com/people"));
    }

    [TestMethod]
    public async Task HttpClient_CanVerifyHttpPostRequestsWereMade()
    {
        var mocker = new AutoMocker();

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await service.PostAsync("https://example.com/api/test", "Some content");

        mocker.VerifyHttpPost("https://example.com/api/test", "Some content", Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPostErrorResponses()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpPost()
            .ReturnsHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PostAsync("https://example.com/api/test", "Stuff");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPutByUrl()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpPut("/people", "data")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PutAsync("https://example.com/people", "data");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPutByExpression()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpPut(r => r.RequestUri!.AbsoluteUri.EndsWith("/people"))
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PutAsync("https://example.com/people", "data");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_SetupHttpPutDoesNotMatchOtherVerbs()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);
        string content = """[{name: "test"}]""";
        mocker.SetupHttpPut("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);
        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await Assert.ThrowsAsync<MockException>(() => service.GetAsync("https://example.com/people"));
    }

    [TestMethod]
    public async Task HttpClient_CanVerifyHttpPutRequestsWereMade()
    {
        var mocker = new AutoMocker();

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await service.PutAsync("https://example.com/api/test", "Some content");

        mocker.VerifyHttpPut("https://example.com/api/test", "Some content", Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPutErrorResponses()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpPut()
            .ReturnsHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PutAsync("https://example.com/api/test", "Stuff");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPatchByUrl()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpPatch("/people", "data")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PatchAsync("https://example.com/people", "data");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPatchByExpression()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpPatch(r => r.RequestUri!.AbsoluteUri.EndsWith("/people"))
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PatchAsync("https://example.com/people", "data");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_SetupHttpPatchDoesNotMatchOtherVerbs()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);
        mocker.SetupHttpPatch("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, "content");
        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await Assert.ThrowsAsync<MockException>(() => service.PutAsync("https://example.com/people", "data"));
    }

    [TestMethod]
    public async Task HttpClient_CanVerifyHttpPatchRequestsWereMade()
    {
        var mocker = new AutoMocker();

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await service.PatchAsync("https://example.com/api/test", "Some content");

        mocker.VerifyHttpPatch("https://example.com/api/test", "Some content", Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpPatchErrorResponses()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpPatch()
            .ReturnsHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.PatchAsync("https://example.com/api/test", "Stuff");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpDeleteByUrl()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpDelete("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.DeleteAsync("https://example.com/people");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpDeleteByExpression()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpDelete(r => r.RequestUri!.AbsoluteUri.EndsWith("/people"))
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.DeleteAsync("https://example.com/people");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_SetupHttpDeleteDoesNotMatchOtherVerbs()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);
        string content = """[{name: "test"}]""";
        mocker.SetupHttpDelete("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);
        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await Assert.ThrowsAsync<MockException>(() => service.GetAsync("https://example.com/people"));
    }

    [TestMethod]
    public async Task HttpClient_CanVerifyHttpDeleteRequestsWereMade()
    {
        var mocker = new AutoMocker();

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await service.DeleteAsync("https://example.com/api/test");

        mocker.VerifyHttpDelete("https://example.com/api/test", Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpDeleteErrorResponses()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpDelete()
            .ReturnsHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.DeleteAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpHeadByUrl()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpHead("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.HeadAsync("https://example.com/people");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpHeadByExpression()
    {
        var mocker = new AutoMocker();
        string content = """[{name: "test"}]""";
        mocker.SetupHttpHead(r => r.RequestUri!.AbsoluteUri.EndsWith("/people"))
            .ReturnsHttpResponse(HttpStatusCode.OK, content);

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.HeadAsync("https://example.com/people");
        var receivedContent = await response.Content.ReadAsStringAsync(TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(content, receivedContent);
    }

    [TestMethod]
    public async Task HttpClient_SetupHttpHeadDoesNotMatchOtherVerbs()
    {
        var mocker = new AutoMocker(MockBehavior.Strict);
        string content = """[{name: "test"}]""";
        mocker.SetupHttpHead("/people")
            .ReturnsHttpResponse(HttpStatusCode.OK, content);
        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await Assert.ThrowsAsync<MockException>(() => service.GetAsync("https://example.com/people"));
    }

    [TestMethod]
    public async Task HttpClient_CanVerifyHttpHeadRequestsWereMade()
    {
        var mocker = new AutoMocker();

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        await service.HeadAsync("https://example.com/api/test");

        mocker.VerifyHttpHead("https://example.com/api/test", Times.Once());
    }

    [TestMethod]
    public async Task HttpClient_CanSetupHttpHeadErrorResponses()
    {
        var mocker = new AutoMocker();

        mocker.SetupHttpHead()
            .ReturnsHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        var service = mocker.CreateInstance<ServiceWithHttpClient>();

        var response = await service.HeadAsync("https://example.com/api/test");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
