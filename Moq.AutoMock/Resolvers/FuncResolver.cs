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

            var invoke = serviceType.GetMethod("Invoke");
            var returnType = invoke?.ReturnType;
            if (returnType == null || returnType == typeof(void))
                return; //I believe Moq will have created a value, we're really about giving it one that returns something from AM

            Expression call = Expression.Call(typeof(FuncResolver), nameof(GetService), null,
                Expression.Constant(am, typeof(AutoMocker)), Expression.Constant(returnType, typeof(Type)));

            var rti = returnType.GetTypeInfo();
            if (rti.IsValueType || rti.IsPrimitive)
                call = Expression.Unbox(call, returnType);
            else
                call = Expression.TypeAs(call, returnType);

            var genericArgs = serviceType.GetGenericArguments();
            var @params = genericArgs.Take(genericArgs.Length - 1).Select(t => Expression.Parameter(t));

            context.Value = Expression.Lambda(serviceType, call, @params).Compile();
        }

        private static object GetService(AutoMocker autoMocker, Type requestType)
        {
            //TODO - what does this really need to be?
            //
            //var methodName = true /*rti.IsInterface || rti.IsAbstract*/
            //    ? nameof(AutoMocker.Get)
            //    : nameof(AutoMocker.CreateInstance);    //TODO - needs tested
            return autoMocker.Get(requestType);
        }
    }
}
