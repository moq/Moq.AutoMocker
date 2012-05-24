using System;
using System.Linq;

namespace Moq.AutoMock
{
    public class AutoMocker
    {
        public T GetInstance<T>()
            where T : class
        {
            var ctor = typeof (T).GetConstructors()[0];
            var arguments = ctor.GetParameters().Select(x => CreateMockOf(x.ParameterType)).ToArray();
            return (T)Activator.CreateInstance(typeof(T), arguments);
        }

        private static object CreateMockOf(Type type)
        {
            var mockType = typeof (Mock<>).MakeGenericType(type);
            var mock = (Mock)Activator.CreateInstance(mockType);
            return mock.Object;
        }
    }
}
