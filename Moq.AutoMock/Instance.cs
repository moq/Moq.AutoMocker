using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                Array array = Array.CreateInstance(type, mocks.Count);
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

        public MockInstance(AutoMocker autoMocker, Type mockType, MockBehavior mockBehavior, BindingFlags bindingFlags)
            : this(CreateMockOf(autoMocker, mockType, mockBehavior, bindingFlags))
        {

        }

        private static Mock CreateMockOf(AutoMocker autoMocker, Type type, MockBehavior mockBehavior, BindingFlags bindingFlags)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                MethodInfo method = typeof(AutoMocker).GetMethods().Single(mi => mi.Name == nameof(AutoMocker.CreateSelfMock) && mi.GetParameters().SingleOrDefault(pi => pi.ParameterType == typeof(bool)) != null);
                MethodInfo genericMethod = method.MakeGenericMethod(type);
                object value = genericMethod.Invoke(autoMocker, new object[] { bindingFlags.HasFlag(BindingFlags.NonPublic) });

                MethodInfo getMethod = typeof(Mock).GetMethod("Get");
                MethodInfo genericGetMethod = getMethod.MakeGenericMethod(type);
                return (Mock)genericGetMethod.Invoke(null, new[] { value });
            }

            var mockType = typeof(Mock<>).MakeGenericType(type);
            var mock = (Mock)Activator.CreateInstance(mockType, mockBehavior);
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
