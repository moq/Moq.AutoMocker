using System;
using System.Collections.Generic;
using System.Linq;

namespace Moq.AutoMock
{
    internal interface IInstance
    {
        object Value { get; }
        bool IsMock { get; }
    }

    internal sealed class MockArrayInstance : IInstance
    {
        private readonly Type type;
        private readonly List<IInstance> mocks;

        public MockArrayInstance(Type type)
        {
            this.type = type;
            mocks = new List<IInstance>();
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

    internal sealed class MockInstance : IInstance
    {
        public MockInstance(Mock value)
        {
            Mock = value;
        }

        public object Value => Mock.Object;

        public Mock Mock { get; }

        public bool IsMock => true;
    }

    internal sealed class RealInstance : IInstance
    {
        public RealInstance(object value)
        {
            Value = value;
        }

        public object Value { get; }
        public bool IsMock => false;
    }
}
