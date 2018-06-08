using Moq.AutoMock.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Moq.AutoMock.Resolvers
{
    public class LazyResolver : IMockResolver
    {
        public void Resolve(MockResolutionContext context)
        {
            var (am, serviceType, value) = context ?? throw new ArgumentNullException(nameof(context));

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
}
