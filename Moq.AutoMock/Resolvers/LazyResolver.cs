using System.Reflection;
using Moq.AutoMock.Extensions;

namespace Moq.AutoMock.Resolvers;

/// <summary>
/// A resolver that resolves Lazy&lt;T&gt; requested types.
/// </summary>
public class LazyResolver : IMockResolver
{
    /// <summary>
    /// Resolves Lazy&lt;T&gt; types.
    /// </summary>
    /// <param name="context">The resolution context.</param>
    public void Resolve(MockResolutionContext context)
    {
        var (am, serviceType, _) = context;

        if (!serviceType.GetTypeInfo().IsGenericType || serviceType.GetGenericTypeDefinition() != typeof(Lazy<>))
            return;

        var returnType = serviceType.GetGenericArguments().Single();
        if (am.TryCompileGetter(typeof(Func<>).MakeGenericType(returnType), out var @delegate))
        {
            var lazyType = typeof(Lazy<>).MakeGenericType(returnType);
            context.Value = Activator.CreateInstance(lazyType, @delegate);
        }
    }
}
