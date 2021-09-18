using System;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock.Resolvers
{
    /// <summary>
    /// A resolver that resolves requested types with Mock&lt;T&gt; instances.
    /// </summary>
    public class MockResolver : IMockResolver
    {
        private readonly MockBehavior _mockBehavior;
        private readonly DefaultValue _defaultValue;
        private readonly bool _callBase;

        /// <summary>
        /// Initializes an instance of <c>MockResolver</c>.
        /// </summary>
        /// <param name="mockBehavior">Behavior of created mock.</param>
        /// <param name="defaultValue">Specifies the behavior to use when returning default values for 
        /// unexpected invocations on loose mocks created by this instance.</param>
        /// <param name="callBase">Whether the base member virtual implementation will be called 
        /// for created mocks if no setup is matched.</param>
        public MockResolver(MockBehavior mockBehavior, DefaultValue defaultValue, bool callBase)
        {
            _mockBehavior = mockBehavior;
            _defaultValue = defaultValue;
            _callBase = callBase;
        }

        /// <summary>
        /// Resolves requested types with Mock instances.
        /// </summary>
        /// <param name="context">The resolution context.</param>
        public void Resolve(MockResolutionContext context)
        {
            if (context.RequestType == typeof(string)) return;

            Type requestType = context.RequestType;
            var mockType = typeof(Mock<>).MakeGenericType(requestType);

            bool mayHaveDependencies = requestType.IsClass
                                       && !typeof(Delegate).IsAssignableFrom(requestType);

            object?[] constructorArgs = Array.Empty<object>();
            if (mayHaveDependencies && 
                context.AutoMocker.TryGetConstructorInvocation(requestType, context.ObjectGraphContext, out ConstructorInfo? ctor, out IInstance[]? arguments))
            {
                constructorArgs = arguments.Select(x => x.Value).ToArray();
                context.AutoMocker.CacheInstances(arguments.Zip(ctor.GetParameters(), (i, p) => (p.ParameterType, i)));
            }

            if (Activator.CreateInstance(mockType, _mockBehavior, constructorArgs) is Mock mock)
            {
                mock.DefaultValue = _defaultValue;
                mock.CallBase = _callBase;
                context.Value = mock;
            }
        }
    }
}
