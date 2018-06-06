using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Moq.AutoMock.Resolvers
{
    public class FuncResolver : IMockResolver
    {
        public void Resolve(MockResolutionContext context)
        {
            var (am, serviceType, value) = context ?? throw new ArgumentNullException(nameof(context));

            if (!typeof(Delegate).IsAssignableFrom(serviceType)) return;

            var stInfo = serviceType.GetTypeInfo();
            if (!stInfo.IsGenericType || !(serviceType.GetGenericTypeDefinition() is Type td)) return;
            if (td.Namespace != nameof(System) || !Regex.IsMatch(td.Name, $"^{nameof(Func<object>)}\\b")) return;

            var genericArgs = serviceType.GetGenericArguments();
            var @params = genericArgs.Take(genericArgs.Length - 1)
                .Select(t => Expression.Parameter(t));
            var returnType = genericArgs.Last();

            if (returnType == null || returnType == typeof(void))
                return; //I believe Moq will have created a value, we're really about giving it one that returns something from AM

            Expression call = Expression.Call(Expression.Constant(am), nameof(AutoMocker.Get), null, Expression.Constant(returnType, typeof(Type)));

            var rti = returnType.GetTypeInfo();
            if (rti.IsValueType || rti.IsPrimitive)
                call = Expression.Unbox(call, returnType);
            else
                call = Expression.TypeAs(call, returnType);

            context.Value = Expression.Lambda(serviceType, call, @params).Compile();
        }
    }
}
