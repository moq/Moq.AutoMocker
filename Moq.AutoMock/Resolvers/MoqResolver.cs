using System;
using System.Collections.Generic;
using System.Text;

namespace Moq.AutoMock.Resolvers
{
    public class MoqResolver : IMockResolver
    {
        private readonly MockBehavior _mockBehavior;
        private readonly DefaultValue _defaultValue;
        private readonly bool _callBase;

        public MoqResolver(MockBehavior mockBehavior, DefaultValue defaultValue, bool callBase)
        {
            _mockBehavior = mockBehavior;
            _defaultValue = defaultValue;
            _callBase = callBase;
        }

        public void Resolve(MockResolutionContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            if (context.Value is null)
            {
                var mockType = typeof(Mock<>).MakeGenericType(context.RequestType);
                context.Value = Activator.CreateInstance(mockType, _mockBehavior);
            }

            if (context.Value is Mock mock)
            {
                mock.DefaultValue = _defaultValue;
                mock.CallBase = _callBase;
            }
        }
    }
}
