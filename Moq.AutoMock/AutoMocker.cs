using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Moq.AutoMock
{
    /// <summary>
    /// An auto-mocking IoC container that generates mock objects using Moq.
    /// </summary>
    public class AutoMocker
    {
        private readonly Dictionary<Type, object> typeMap = new Dictionary<Type, object>();
        private readonly ConstructorSelector constructorSelector = new ConstructorSelector();

        /// <summary>
        /// Constructs an instance from known services. Any dependancies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <typeparam name="T">A concrete type</typeparam>
        /// <returns>An instance of T with all constructor arguments derrived from services 
        /// setup in the container.</returns>
        public T GetInstance<T>()
            where T : class
        {
            var arguments = CreateArguments<T>();
            return (T)Activator.CreateInstance(typeof(T), arguments);
        }

        private object[] CreateArguments<T>() where T : class
        {
            var ctor = constructorSelector.SelectFor(typeof (T));
            var arguments = ctor.GetParameters().Select(x => GetObjectFor(x.ParameterType)).ToArray();
            return arguments;
        }

        /// <summary>
        /// Constructs a self-mock from the services available in the container. A self-mock is
        /// a concrete object that has virtual and abstract members mocked. The purpose is so that
        /// you can test the majority of a class but mock out a resource. This is great for testing
        /// abstract classes, or avoiding breaking cohesion even further with a non-abstract class.
        /// </summary>
        /// <typeparam name="T">The instance that you want to build</typeparam>
        /// <returns>An instance with virtual and abstract members mocked</returns>
        public T GetSelfMock<T>() where T : class
        {
            var arguments = CreateArguments<T>();
            return new Mock<T>(arguments).Object;
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

        /// <summary>
        /// Adds an intance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="service"></param>
        public void Use<TService>(TService service)
        {
            typeMap.Add(typeof(TService), service);
        }

        /// <summary>
        /// Adds a mock object to the container that implements TService.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="setup">A shortcut for Mock.Of's syntax</param>
        public void Use<TService>(Expression<Func<TService, bool>> setup) 
            where TService : class
        {
            Use(Mock.Of(setup));
        }

        /// <summary>
        /// Searches and retrieves an object from the container that matches TService. This can be
        /// a service setup explicitly via `.Use()` or implicitly with `.GetInstance()`.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <returns>The object that implements TService</returns>
        public TService Extract<TService>()
        {
            return (TService) typeMap[typeof (TService)];
        }

        /// <summary>
        /// This is a shortcut for calling `mock.VerifyAll()` on every mock that we have.
        /// </summary>
        public void VerifyAll()
        {
            foreach (var pair in typeMap)
            {
                var value = GetMockSafely(pair.Key, pair.Value);
                if (value != null)
                    (value).VerifyAll();
            }
        }

        private static Mock GetMockSafely(Type type, object value)
        {
            if (!value.GetType().GetProperties().Any(x => x.Name == "Mock"))
                return null;

            var method = typeof (Mock).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);
            var targetMethod = method.MakeGenericMethod(type);
            try
            {
                return (Mock) targetMethod.Invoke(null, new[] {value});
            }
            catch
            {
                return null;
            }
        }
    }
}
