using System;

namespace Moq.AutoMock.Resolvers
{
    public class MockResolutionContext
    {
        public AutoMocker AutoMocker { get; set; }
        public Type RequestType { get; set; }
        public object Value { get; set; }

        public void Deconstruct(out AutoMocker autoMocker, out Type type, out object value)
        {
            autoMocker = AutoMocker;
            type = RequestType;
            value = Value;
        }
    }
}
