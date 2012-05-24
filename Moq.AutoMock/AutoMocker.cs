using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

        private object CreateMockOf(Type type)
        {
            var mockType = typeof (Mock<>).MakeGenericType(type);
            var mock = (Mock)Activator.CreateInstance(mockType);
            return (typeMap[type] = mock.Object);
        }

        public void Use<TService>(TService service)
        {
            typeMap.Add(typeof(TService), service);
        }

        public void Use<TService>(Expression<Func<TService, bool>> setup) 
            where TService : class
        {
            Use(Mock.Of(setup));
        }

        public TService Extract<TService>()
        {
            return (TService) typeMap[typeof (TService)];
        }
    }
}
