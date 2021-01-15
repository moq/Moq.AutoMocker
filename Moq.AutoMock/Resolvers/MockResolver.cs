﻿using System;

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
            if (context is null) throw new ArgumentNullException(nameof(context));

            if (!(context.Value is null)) return;

            Type requestType = context.RequestType;
            var mockType = typeof(Mock<>).MakeGenericType(requestType);

            bool mayHaveDependencies = requestType.IsClass
                                       && !typeof(Delegate).IsAssignableFrom(requestType);

            object?[] constructorArgs = mayHaveDependencies
                ? context.AutoMocker.CreateArguments(requestType, context.ObjectGraphContext)
                : Array.Empty<object>();

            if (Activator.CreateInstance(mockType, _mockBehavior, constructorArgs) is Mock mock)
            {
                mock.DefaultValue = _defaultValue;
                mock.CallBase = _callBase;
                context.Value = mock;
            }
        }
    }
}
