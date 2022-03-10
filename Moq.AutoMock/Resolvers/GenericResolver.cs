namespace Moq.AutoMock.Resolvers;
/// <summary>
/// A generic resolver that forwards to a simple callback delegate.
/// </summary>
public class GenericResolver : IMockResolver
{
    /// <summary>
    /// Creates a new instance 
    /// </summary>
    /// <param name="callback">The delegate to invoke</param>
    public GenericResolver(Action<MockResolutionContext> callback)
    {
        Callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    private Action<MockResolutionContext> Callback { get; }

    /// <summary>
    /// Resolves types by forwardeing calls to the provided callback.
    /// </summary>
    /// <param name="context">The context</param>
    public void Resolve(MockResolutionContext context)
        => Callback(context);
}
