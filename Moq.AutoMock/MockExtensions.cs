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
        /// Specifies a setup on the mocked type for a call to a void method. 
        /// All parameters are filled with <see cref ="It.IsAny" /> according to the parameter's type.
        /// </summary>
        /// <remarks>
        /// This may only be used on methods that are not overloaded.
        /// </remarks>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="mock">The mock</param>
        /// <param name="methodName">The name of the expected method invocation.</param>
        /// <exception cref="ArgumentNullException">When mock or methodName is null.</exception>
        /// <exception cref="MissingMethodException">Thrown when no method with methodName is found.</exception>
        /// <exception cref="AmbiguousMatchException">Thrown when more that one method matches the passed method name.</exception>
        /// <returns></returns>
        public static ISetup<T> SetupWithAny<T>(this Mock<T> mock, string methodName)
            where T : class
        {
            if (mock is null)
            {
                throw new ArgumentNullException(nameof(mock));
            }

            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            LambdaExpression lambdaExpression = GetExpression<T>(methodName);

            MethodInfo setupMethod = mock.GetType().GetMethods()
                .Single(x => x.Name == nameof(Mock<object>.Setup) && x.ReturnType.GetGenericArguments().Length == 1);
            return (ISetup<T>)setupMethod.Invoke(mock, new object[] { lambdaExpression })!;
        }

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a non-void (value-returning) method. 
        /// All parameters are filled with <see cref ="It.IsAny" /> according to the parameter's type.
        /// </summary>
        /// <remarks>
        /// This may only be used on methods that are not overloaded.
        /// </remarks>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">The return type of the method</typeparam>
        /// <param name="mock">The mock</param>
        /// <param name="methodName">The name of the expected method invocation.</param>
        /// <exception cref="ArgumentNullException">When mock or methodName is null.</exception>
        /// <exception cref="MissingMethodException">Thrown when no method with methodName is found.</exception>
        /// <exception cref="AmbiguousMatchException">Thrown when more that one method matches the passed method name.</exception>
        /// <returns></returns>
        public static ISetup<T, TResult> SetupWithAny<T, TResult>(this Mock<T> mock, string methodName)
            where T : class
        {
            if (mock is null)
            {
                throw new ArgumentNullException(nameof(mock));
            }

            if (methodName is null)
            {
                throw new ArgumentNullException(nameof(methodName));
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
            var matchingMethods = typeof(T).GetMethods().Where(x => string.Equals(x.Name, methodName, StringComparison.Ordinal)).ToArray();
            MethodInfo method = matchingMethods.Length switch
            {
                0 => throw new MissingMethodException(typeof(T).Name, methodName),
                1 => matchingMethods[0],
                _ => throw new AmbiguousMatchException($"Cannot create a Setup on method '{methodName}'. {nameof(SetupWithAny)} does not support methods with multiple overloads."),
            };


            //Build up the expression to pass to the Setup method
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
