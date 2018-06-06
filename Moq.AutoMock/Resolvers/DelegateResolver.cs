using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Moq.AutoMock.Resolvers
{
    public class DelegateResolver : IMockResolver
    {
        public void Resolve(MockResolutionContext context)
        {
            var (am, serviceType, value) = context ?? throw new ArgumentNullException(nameof(context));

            if (!typeof(Delegate).GetTypeInfo().IsAssignableFrom(serviceType.GetTypeInfo()))
                return; //We aren't a delegate

            var invoke = serviceType.GetMethod("Invoke");
            var returnType = invoke?.ReturnType;
            if (returnType == null || returnType == typeof(void))
                return; //I believe Moq will have created a value, we're really about giving it one that returns something from AM

            //TODO - what does this really need to be?
            var rti = returnType.GetTypeInfo();
            var methodName = true /*rti.IsInterface || rti.IsAbstract*/
                ? nameof(AutoMocker.Get)
                : nameof(AutoMocker.CreateInstance);    //TODO - needs tested

            Expression call = Expression.Call(Expression.Constant(am, typeof(AutoMocker)), methodName, null, Expression.Constant(returnType, typeof(Type)));
            if (rti.IsValueType || rti.IsPrimitive)
                call = Expression.Unbox(call, returnType);
            else
                call = Expression.TypeAs(call, returnType);

            var @params = invoke.GetParameters().Select(pi => Expression.Parameter(pi.ParameterType));
            var lambda = Expression.Lambda(serviceType, call, @params)
                .Compile();

            context.Value = lambda;
        }
    }
}
