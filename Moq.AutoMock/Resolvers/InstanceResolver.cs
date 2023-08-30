namespace Moq.AutoMock.Resolvers;

/// <summary>
/// A resolver that resolves requested types with a created instance.
/// </summary>
public class InstanceResolver : IMockResolver
{
    /// <summary>
    /// Resolves requested types with created instances.
    /// </summary>
    /// <param name="context">The resolution context.</param>
    public void Resolve(MockResolutionContext context)
    {
        if (context.AutoMocker.CreateInstanceInternal(context.RequestType, context.ObjectGraphContext) is { } instance)
        {
            context.Value = instance;
        }
    }
}
