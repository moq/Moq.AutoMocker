using System;
using Moq.AutoMock.Extensions;

namespace Moq.AutoMock.Resolvers;

/// <summary>
/// A resolver that resolves Func&lt;&gt; dependency types
/// </summary>
public class FuncResolver : IMockResolver
{
    /// <summary>
    /// Resolves requested Func&lt;&gt; types.
    /// </summary>
    /// <param name="context">The resolution context.</param>
    public void Resolve(MockResolutionContext context)
    {
        var (am, serviceType, _) = context;

        if (am.TryCompileGetter(serviceType, out var @delegate))
            context.Value = @delegate;
    }
}
