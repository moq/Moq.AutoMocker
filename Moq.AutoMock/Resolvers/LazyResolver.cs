using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

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

            Expression call = Expression.Call(Expression.Constant(am), nameof(AutoMocker.Get), null, Expression.Constant(returnType, typeof(Type)));

            var rti = returnType.GetTypeInfo();
            if (rti.IsValueType || rti.IsPrimitive)
                call = Expression.Unbox(call, returnType);
            else
                call = Expression.TypeAs(call, returnType);

            var func = Expression.Lambda(typeof(Func<>).MakeGenericType(returnType), call).Compile();

            var lazyType = typeof(Lazy<>).MakeGenericType(returnType);
            context.Value = Activator.CreateInstance(lazyType, func);
        }
    }
}
