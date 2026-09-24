using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Moq.AutoMocker.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class HttpClientFactoryResolverSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<bool> referencesHttp = context.CompilationProvider
            .Select(static (compilation, _) => compilation.ReferencedAssemblyNames
                .Any(assembly => assembly.Name.StartsWith("Microsoft.Extensions.Http", StringComparison.Ordinal)));

        context.RegisterSourceOutput(referencesHttp, static (productionContext, shouldGenerate) =>
        {
            if (shouldGenerate)
                productionContext.AddSource("AutoMockerHttpClientFactoryExtensions.g.cs", Source);
        });
    }

    private const string Source =
        """
        #nullable enable
        using System;
        using System.Collections.Concurrent;
        using System.Net.Http;
        using Microsoft.Extensions.Http;
        using Moq.AutoMock.Http;
        using Moq.AutoMock.Resolvers;

        namespace Moq.AutoMock;

        public static class AutoMockerHttpClientFactoryExtensions
        {
            public static AutoMocker WithHttpClientFactory(this AutoMocker mocker)
            {
                if (mocker is null)
                    throw new ArgumentNullException(nameof(mocker));

                mocker.InsertResolverAfter<CacheResolver>(new HttpClientFactoryResolver());
                return mocker;
            }

            private sealed class HttpClientFactoryResolver : IMockResolver
            {
                public void Resolve(MockResolutionContext context)
                {
                    if (context.RequestType == typeof(IHttpClientFactory))
                        context.Value = new TestableHttpClientFactory(context.AutoMocker);
                }
            }

            private sealed class TestableHttpClientFactory(AutoMocker autoMocker) : IHttpClientFactory
            {
                private readonly ConcurrentDictionary<string, HttpClient> _clients = new(StringComparer.Ordinal);

                public HttpClient CreateClient(string name)
                    => _clients.GetOrAdd(name ?? string.Empty, _ =>
                        new HttpClient(autoMocker.GetMock<HttpMessageHandler>().Object, disposeHandler: false));
            }
        }
        """;
}
