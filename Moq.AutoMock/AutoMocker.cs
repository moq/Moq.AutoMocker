using System;
using System.Collections.Generic;
using System.Linq;

namespace Moq.AutoMock
{
    public class AutoMocker
    {
        private readonly Dictionary<Type, object> typeMap = new Dictionary<Type, object>();
        private readonly ConstructorSelector constructorSelector = new ConstructorSelector();

        public T GetInstance<T>()
            where T : class
        {
            var ctor = constructorSelector.SelectFor(typeof (T));
            var arguments = ctor.GetParameters().Select(x => GetObjectFor(x.ParameterType)).ToArray();
            return (T)Activator.CreateInstance(typeof(T), arguments);
        }

        private object GetObjectFor(Type type)
        {
            return typeMap.ContainsKey(type) ? typeMap[type] : CreateMockOf(type);
        }

        private static object CreateMockOf(Type type)
        {
            var mockType = typeof (Mock<>).MakeGenericType(type);
            var mock = (Mock)Activator.CreateInstance(mockType);
            return mock.Object;
        }

        public void Use<TService>(TService service)
        {
            typeMap.Add(typeof(TService), service);
        }
    }
}
