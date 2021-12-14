namespace Moq.AutoMock.Resolvers;

/// <summary>
/// Base interface for all mock resolvers.
/// </summary>
public interface IMockResolver
{
    /// <summary>
    /// Resolve a dependency.
    /// </summary>
    /// <param name="context">The context to be used while resolving the dependency.</param>
    void Resolve(MockResolutionContext context);
}
