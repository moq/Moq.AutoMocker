using System.Net.Http;

namespace Moq.AutoMock.Tests;

public class ServiceWithHttpClient(HttpClient httpClient)
{
    public HttpClient HttpClient => httpClient;

    public Task<HttpResponseMessage> GetAsync(string url)
        => httpClient.GetAsync(url);

    public Task<HttpResponseMessage> PostAsync(string url, string content)
        => PostAsync(url, new StringContent(content));

    public Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        => httpClient.PostAsync(url, content);

    public Task<HttpResponseMessage> PutAsync(string url, string content)
        => PutAsync(url, new StringContent(content));

    public Task<HttpResponseMessage> PutAsync(string url, HttpContent content)
        => httpClient.PutAsync(url, content);

    public Task<HttpResponseMessage> DeleteAsync(string url)
        => httpClient.DeleteAsync(url);

    public Task<HttpResponseMessage> HeadAsync(string url)
        => httpClient.SendAsync(new HttpRequestMessage()
        {
            Method = HttpMethod.Head,
            RequestUri = new Uri(url)
        });
}
