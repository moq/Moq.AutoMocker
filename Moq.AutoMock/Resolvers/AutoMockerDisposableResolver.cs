namespace Moq.AutoMock.Resolvers;

/// <summary>
/// A resolver that resolves instances for <see cref="IAutoMockerDisposable"/>.
/// </summary>
public class AutoMockerDisposableResolver : IMockResolver
{
    /// <summary>
    /// Resolve the <see cref="IAutoMockerDisposable"/> if one has not been found.
    /// </summary>
    /// <param name="context"></param>
    public void Resolve(MockResolutionContext context)
    {
        if (context.RequestType.IsAssignableFrom(typeof(IAutoMockerDisposable)))
        {
            context.Value = new AutoMockerDisposable(context.AutoMocker);
        }
    }
}
