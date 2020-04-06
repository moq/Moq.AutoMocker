using System;
using System.Linq.Expressions;
using System.Reflection;
using Moq.Language.Flow;

namespace Moq.AutoMock
{
    public static class CastChecker
    {
        /// <summary>
        /// We are expecting expression to be m => m.Setup(setup). We will assume this structure
        /// and check if the inner setup is Converted (casted) to Object
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool DoesContainCastToObject<TService>(Expression<Func<Mock<TService>, ISetup<TService, object>>> expression) 
            where TService : class
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));

            //expression is assumed to be a lambda so the cast here would cause an exception if it was not the expected format
            var lambdaExpression = (LambdaExpression)expression;

            //m.Setup(setup)
            //lambdaExpression.Body is assumed to be a MethodCallExpression so the cast here would cause an exception if it was not the expected format
            var methodCallExpression = (MethodCallExpression)lambdaExpression.Body;

            //only one argument in m.Setup(setup)
            var setupArgument = (ConstantExpression)((MemberExpression)methodCallExpression.Arguments[0]).Expression;

            //get the setup parameter from the anonymous type
            var argumentValueType = setupArgument.Value.GetType();
            var p = argumentValueType.GetField("setup");

            //it should be a lambda this is the lambda that is the setup specified
            var setup = (LambdaExpression)p.GetValue(setupArgument.Value);

            //Does its body Convert to System.Object?
            if (setup.Body.NodeType == ExpressionType.Convert && setup.Body.Type == typeof(object))
            {
                return true;
            }

            //does not contain a Cast To Object
            return false;
        }

        public static bool DoesReturnPrimitive<TService>(Expression<Func<TService, object>> expression)
            where TService : class
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));

            //expression is assumed to be a lambda so the cast here would cause an exception if it was not the expected format
            var lambdaExpression = (LambdaExpression)expression;

            //Does its body Convert to System.Object?
            if (lambdaExpression.Body.NodeType == ExpressionType.Convert && lambdaExpression.Body.Type == typeof(object))
            {
                return true;
            }

            //does not contain a Cast To Object
            return false;
        }

    }
}