using System;

namespace Moq.AutoMock
{
    interface IInstance
    {
        object Value { get; }
        bool IsMock { get; }
    }

    class MockInstance : IInstance
    {
        public MockInstance(Mock value)
        {
            Mock = value;
        }

        public MockInstance(Type mockType, MockBehavior defaultBehavior)
            :this(CreateMockOf(mockType, defaultBehavior))
        {
        }

        private static Mock CreateMockOf(Type type, MockBehavior defaultBehavior)
        {
            var mockType = typeof (Mock<>).MakeGenericType(type);
            var mock = (Mock) Activator.CreateInstance(mockType, new object[] { defaultBehavior});
            return mock;
        }

        public object Value
        {
            get { return Mock.Object; }
        }

        public Mock Mock { get; private set; }

        public bool IsMock { get { return true; } }
    }

    class RealInstance : IInstance
    {
        public RealInstance(object value)
        {
            Value = value;
        }

        public object Value { get; private set; }
        public bool IsMock { get { return false; } }
    }
}
