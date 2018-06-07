using Moq.AutoMock.Extensions;
using System;

namespace Moq.AutoMock.Resolvers
{
    public class FuncResolver : IMockResolver
    {
        public void Resolve(MockResolutionContext context)
        {
            var (am, serviceType, value) = context ?? throw new ArgumentNullException(nameof(context));

            if (am.TryCompileGetter(serviceType, out var @delegate))
                context.Value = @delegate;
        }
    }
}
