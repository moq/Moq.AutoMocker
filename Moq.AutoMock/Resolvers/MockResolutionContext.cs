using System;

namespace Moq.AutoMock.Resolvers
{
    public class MockResolutionContext
    {
        public MockResolutionContext(AutoMocker autoMocker, Type requestType)
            : this(autoMocker, requestType, null) { }
        public MockResolutionContext(AutoMocker autoMocker, Type requestType, object initialValue)
        {
            AutoMocker = autoMocker ?? throw new ArgumentNullException(nameof(autoMocker));
            RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
            Value = initialValue;
        }

        public AutoMocker AutoMocker { get; }
        public Type RequestType { get; }
        public object Value { get; set; }

        public void Deconstruct(out AutoMocker autoMocker, out Type type, out object value)
        {
            autoMocker = AutoMocker;
            type = RequestType;
            value = Value;
        }
    }
}
