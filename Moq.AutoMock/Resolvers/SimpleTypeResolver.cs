namespace Moq.AutoMock.Resolvers;

/// <summary>
/// A base type resolver for handling simple type resolution.
/// </summary>
/// <typeparam name="T">The type to resolve.</typeparam>
public abstract class SimpleTypeResolver<T> : IMockResolver
{
    private static Lazy<HashSet<Type>> Interfaces { get; } = new(() => [.. typeof(T).GetInterfaces()]);

    /// <summary>
    /// A flag indicating if <see cref="GetValue(MockResolutionContext)"/> should be called if the the requested type is a base type of <typeparamref name="T"/>. Defaults to true.
    /// </summary>
    protected bool IncludeBaseTypes { get; set; } = true;

    /// <summary>
    /// A flag indicating if <see cref="GetValue(MockResolutionContext)"/> should be called if the the requested type is an interface implemented by <typeparamref name="T"/>. Defaults to true.
    /// </summary>
    protected bool IncludeInterfaces { get; set; } = true;

    /// <inheritdoc />
    public void Resolve(MockResolutionContext context)
    {
        if (context.RequestType == typeof(T) ||
            (IncludeBaseTypes && typeof(T).IsAssignableFrom(context.RequestType)) ||
            (IncludeInterfaces && Interfaces.Value.Contains(context.RequestType)))
        {
            context.Value = GetValue(context);
        }
    }

    /// <summary>
    /// Gets the value to return for the resolved type.
    /// </summary>
    /// <param name="context">The resolution context.</param>
    /// <returns>The value to return for the resolved type.</returns>
    protected abstract T GetValue(MockResolutionContext context);
}
