using System.Linq.Expressions;

namespace Moq.AutoMock;

internal static class CastChecker
{
    public static bool DoesReturnPrimitive<TService>(Expression<Func<TService, object>> expression)
        where TService : class
    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));

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
