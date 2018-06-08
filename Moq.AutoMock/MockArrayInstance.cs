using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Moq.AutoMock
{
    internal sealed class MockArrayInstance : IInstance
    {
        private readonly Type _type;
        private readonly List<IInstance> _mocks = new List<IInstance>();

        public MockArrayInstance(Type type) => _type = type;

        public object Value
        {
            get
            {
                int i = 0;
                Array array = Array.CreateInstance(_type, _mocks.Count);
                foreach (IInstance instance in _mocks)
                    array.SetValue(instance.Value, i++); 
                return array;
            }
        }

        public bool IsMock { get { return _mocks.Any(m => m.IsMock); } }

        public void Add(IInstance instance)
        {
            _mocks.Add(instance);
        }
    }
}
