# Moq.AutoMock documentation

| Reference | APIs |
| --- | --- |
| [AutoMocker](AutoMocker.md) | Construction, registration, retrieval, self-mocks, combining services, disposal |
| [Setup and verification](SetupAndVerification.md) | `Setup`, `SetupSequence`, `SetupAllProperties`, `SetupWithAny`, `Verify`, `VerifyAll`, `MockExtensions` |
| [Resolvers](Resolvers.md) | Resolver pipeline, built-in resolvers, resolution contexts |
| [Diagnostics and constructor tests](Diagnostics.md) | `ObjectCreationException`, `ConstructorTestsAttribute`, `TestGenerationBehavior` |
| [HTTP](HttpClient.md) | `MockHttpMessageHandlerExtensions`, `IHttpMessageHandler`, `RequestMatcher` |
| [Generated extensions](GeneratedExtensions.md) | Conditional extension signatures, grouped keyed-service overloads, telemetry accessors |
| [Source generators](SourceGenerators.md) | Conditional options, logging, time, telemetry, keyed services, metrics, HTTP factory extensions |
| [Compiler integration](CompilerIntegration.md) | Public Roslyn generator, analyzer, diagnostic, and model APIs |
| [Package validation](PackageValidation.md) | Contributor guidance for compatibility checks |

Overloads are grouped by method. Signatures omit `public` and extension receivers where noted. Examples use application-specific types such as `IStore` and `Store`; replace these with your own types.

Start with `using Moq.AutoMock;` and `using Moq;`. HTTP extensions also require `using Moq.AutoMock.Http;`.
