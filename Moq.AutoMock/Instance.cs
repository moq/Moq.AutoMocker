using System;
using System.Collections.Generic;
using System.Linq;

namespace Moq.AutoMock
{
    interface IInstance
    {
        object Value { get; }
        bool IsMock { get; }
    }

    class MockArrayInstance : IInstance
    {
        private readonly Type type;
        private readonly List<IInstance> mocks;

        public MockArrayInstance(Type type)
        {
            this.type = type;
            mocks = new List<IInstance>();
        }

        public IEnumerable<IInstance> Mocks
        {
            get { return mocks; }
        }

        public object Value
        {
            get
            {
                int i = 0;
                Array array = Array.CreateInstance(type,mocks.Count);
                foreach (IInstance instance in mocks)
                    array.SetValue(instance.Value, i++); 
                return array;
            }
        }

        public bool IsMock { get { return mocks.Any(m => m.IsMock); } }

        public void Add(IInstance instance)
        {
            mocks.Add(instance);
        }
    }

    class MockInstance : IInstance
    {
        public MockInstance(Mock value)
        {
            Mock = value;
        }

        public MockInstance(Type mockType)
            :this(CreateMockOf(mockType))
        {
        }

        private static Mock CreateMockOf(Type type)
        {
            var mockType = typeof (Mock<>).MakeGenericType(type);
            var mock = (Mock) Activator.CreateInstance(mockType);
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
