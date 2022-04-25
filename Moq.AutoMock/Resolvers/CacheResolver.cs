namespace Moq.AutoMock.Resolvers;

/// <summary>
/// Provides the cache used by AutoMocker.
/// </summary>
public class CacheResolver : IMockResolver
{
    internal Dictionary<Type, IInstance> TypeMap { get; } = new();

    /// <inheritdoc />
    public void Resolve(MockResolutionContext context)
    {
        if (TypeMap.TryGetValue(context.RequestType, out IInstance instance))
        {
            context.Value = instance;
        }
    }
}
