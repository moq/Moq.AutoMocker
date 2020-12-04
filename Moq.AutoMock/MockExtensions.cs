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
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mock"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static ISetup<T> SetupAll<T>(this Mock<T> mock, string methodName)
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
        /// TODO
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="mock"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static ISetup<T, TResult> SetupAll<T, TResult>(this Mock<T> mock, string methodName)
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
            if (method is null) throw new Exception("Boom");

            var itType = typeof(Moq.It);
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
