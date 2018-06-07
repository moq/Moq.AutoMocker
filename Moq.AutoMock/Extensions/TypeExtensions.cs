using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Moq.AutoMock.Extensions
{
    public static class TypeExtensions
    {
        internal static bool TryCompileGetter(this AutoMocker autoMocker, Type funcType, out Delegate @delegate)
        {
            @delegate = null;
            var stInfo = funcType.GetTypeInfo();
            if (!typeof(Delegate).IsAssignableFrom(funcType)
                || !stInfo.IsGenericType || !(funcType.GetGenericTypeDefinition() is Type td)
                || td.Namespace != nameof(System) || !Regex.IsMatch(td.Name, $"^{nameof(Func<object>)}\\b"))
                return false;

            var genericArgs = funcType.GetGenericArguments();
            var @params = genericArgs.Take(genericArgs.Length - 1)
                .Select(t => Expression.Parameter(t));
            var returnType = genericArgs.Last();

            Expression call = Expression.Call(Expression.Constant(autoMocker), nameof(AutoMocker.Get), null, Expression.Constant(returnType, typeof(Type)));

            var rti = returnType.GetTypeInfo();
            if (rti.IsValueType || rti.IsPrimitive)
                call = Expression.Unbox(call, returnType);
            else
                call = Expression.TypeAs(call, returnType);

            @delegate = Expression.Lambda(funcType, call, @params).Compile();
            return @delegate != null;
        }
    }
}
