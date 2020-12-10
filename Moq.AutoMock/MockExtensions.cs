using Moq.Language.Flow;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Moq.AutoMock
{
    /// <summary>
    /// TODO
    /// </summary>
    public static class MockExtensions
    {
        /// <summary>
        /// Specifies a setup on the mocked type for a call to a non-void (value-returning) method.
        /// All parameters are filled with <see cref ="It.IsAny" /> according to the parameter's type.
        /// </summary>
        /// <remarks>
        /// This may only be used on methods that are not overloaded.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="mock">The mock</param>
        /// <param name="methodName">The name of the expected method invocation.</param>
        /// <returns></returns>
        public static ISetup<T> SetupWithAny<T>(this Mock<T> mock, string methodName)
            where T : class
        {
            if (mock is null)
            {
                throw new ArgumentNullException(nameof(mock));
            }

            LambdaExpression lambdaExpression = GetExpression<T>(methodName);

            MethodInfo setupMethod = mock.GetType().GetMethods()
                .Single(x => x.Name == nameof(Mock<object>.Setup) && x.ReturnType.GetGenericArguments().Length == 1);
            return (ISetup<T>)setupMethod.Invoke(mock, new object[] { lambdaExpression })!;
        }

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a void method. 
        /// All parameters are filled with <see cref ="It.IsAny" /> according to the parameter's type.
        /// </summary>
        /// <remarks>
        /// This may only be used on methods that are not overloaded.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="mock"></param>
        /// <param name="methodName">The name of the expected method invocation.</param>
        /// <returns></returns>
        public static ISetup<T, TResult> SetupWithAny<T, TResult>(this Mock<T> mock, string methodName)
            where T : class
        {
            if (mock is null)
            {
                throw new ArgumentNullException(nameof(mock));
            }

            LambdaExpression lambdaExpression = GetExpression<T>(methodName);

            //Invoke the setup method
            MethodInfo setupMethod = mock.GetType().GetMethods()
                .Single(x => x.Name == nameof(Mock<object>.Setup) && x.ReturnType.GetGenericArguments().Length == 2);
            var ret = setupMethod.MakeGenericMethod(typeof(TResult)).Invoke(mock, new object[] { lambdaExpression });
            return (ISetup<T, TResult>)ret!;
        }

        private static LambdaExpression GetExpression<T>(string methodName)
        {
            //Build up the expression to pass to the Setup method
            MethodInfo method = typeof(T).GetMethod(methodName)!;
            if (method is null) throw new MissingMethodException(typeof(T).Name, methodName);

            var itType = typeof(It);
            var isAnyMethod = itType.GetMethod(nameof(It.IsAny));

            var parameterExpressions = method.GetParameters()
                .Select(parameter =>
                {
                    var closeAnyMethod = isAnyMethod!.MakeGenericMethod(parameter.ParameterType);
                    return Expression.Call(null, closeAnyMethod);
                })
                .ToArray();

            var xExpression = Expression.Parameter(typeof(T), "x");
            var methodCallExpression = Expression.Call(xExpression, method, parameterExpressions);
            return Expression.Lambda(methodCallExpression, false, new[] { xExpression });
        }
    }

}
