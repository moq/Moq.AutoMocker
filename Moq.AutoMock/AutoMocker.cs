using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq.Language.Flow;

namespace Moq.AutoMock
{
    /// <summary>
    /// An auto-mocking IoC container that generates mock objects using Moq.
    /// </summary>
    public partial class AutoMocker
    {
        private readonly Dictionary<Type, IInstance> typeMap = new Dictionary<Type, IInstance>();
        private readonly ConstructorSelector constructorSelector = new ConstructorSelector();
        private CastChecker castChecker = new CastChecker();
        /// <summary>
        /// Constructs an instance from known services. Any dependancies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <typeparam name="T">A concrete type</typeparam>
        /// <returns>An instance of T with all constructor arguments derrived from services 
        /// setup in the container.</returns>
        public T CreateInstance<T>()
            where T : class
        {
            var arguments = CreateArguments<T>();
            try
            {
                return (T) Activator.CreateInstance(typeof (T), arguments);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException.PreserveStackTrace();
            }
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
        public T CreateSelfMock<T>() where T : class
        {
            var arguments = CreateArguments<T>();
            return new Mock<T>(arguments).Object;
        }

        private object GetObjectFor(Type type)
        {
            var instance = typeMap.ContainsKey(type) ? typeMap[type] : CreateMockObjectAndStore(type);
            return instance.Value;
        }

        private Mock GetOrMakeMockFor(Type type)
        {
            if (!typeMap.ContainsKey(type) || !typeMap[type].IsMock)
            {
                typeMap[type] = new MockInstance(type);
            }
            return ((MockInstance) typeMap[type]).Mock;
        }

        private IInstance CreateMockObjectAndStore(Type type)
        {
            if (type.IsArray)
            {
                Type elmType = type.GetElementType();

                MockArrayInstance instance = new MockArrayInstance(elmType);
                if (typeMap.ContainsKey(elmType))
                    instance.Add(typeMap[elmType]);
                return typeMap[type] = instance;
            }
            return (typeMap[type] = new MockInstance(type));
        }

        /// <summary>
        /// Adds an intance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="service"></param>
        public void Use<TService>(TService service)
        {
            typeMap[typeof(TService)] = new RealInstance(service);
        }

        /// <summary>
        /// Adds an intance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="mockedService"></param>
        public void Use<TService>(Mock<TService> mockedService)
            where TService : class
        {
            typeMap[typeof(TService)] = new MockInstance(mockedService);
        }

        /// <summary>
        /// Adds a mock object to the container that implements TService.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="setup">A shortcut for Mock.Of's syntax</param>
        public void Use<TService>(Expression<Func<TService, bool>> setup) 
            where TService : class
        {
            Use(Mock.Get(Mock.Of(setup)));
        }

        /// <summary>
        /// Searches and retrieves an object from the container that matches TService. This can be
        /// a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <returns>The object that implements TService</returns>
        public TService Get<TService>()
        {
            IInstance instance;
            if (!typeMap.TryGetValue(typeof(TService), out instance))
                instance = CreateMockObjectAndStore(typeof(TService));

            return (TService) typeMap[typeof (TService)].Value;
        }

        /// <summary>
        /// Searches and retrieves the mock that the container uses for TService.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <exception cref="ArgumentException">if the requested object wasn't a Mock</exception>
        /// <returns>a mock that </returns>
        public Mock<TService> GetMock<TService>() where TService : class
        {
            IInstance instance;
            if (!typeMap.TryGetValue(typeof(TService), out instance))
                instance = CreateMockObjectAndStore(typeof(TService));

            if (!instance.IsMock)
                throw new ArgumentException(string.Format("Registered service `{0}` was not a mock", Get<TService>().GetType()));

            var mockInstance = (MockInstance) instance;
            return (Mock<TService>) mockInstance.Mock;
        }

        /// <summary>
        /// This is a shortcut for calling `mock.VerifyAll()` on every mock that we have.
        /// </summary>
        public void VerifyAll()
        {
            foreach (var pair in typeMap)
            {
                if (pair.Value.IsMock)
                    (((MockInstance)pair.Value).Mock).VerifyAll();
            }
        }

        /// <summary>
        /// Shortcut for mock.Setup(...), creating the mock when necessary.
        /// </summary>
        public ISetup<TService, object> Setup<TService>(Expression<Func<TService, object>> setup)
            where TService : class
        {
            Func<Mock<TService>, ISetup<TService, object>> func = m => m.Setup(setup);
            Expression<Func<Mock<TService>, ISetup<TService, object>>> expression = m => m.Setup(setup);
            //check if Func results in a cast to object (boxing). If so then the user should have used the Setup overload that
            //specifies TReturn for value types
            if (castChecker.DoesContainCastToObject(expression))
            {
                throw new NotSupportedException("Use the Setup overload that allows specifying TReturn if the setup returns a value type");
            }

            return Setup<ISetup<TService, object>, TService>(func);
        }


        /// <summary>
        /// Shortcut for mock.Setup(...), creating the mock when necessary.
        /// </summary>
        public ISetup<TService> Setup<TService>(Expression<Action<TService>> setup)
            where TService : class
        {
            return Setup<ISetup<TService>, TService>(m => m.Setup(setup));
        }

        /// <summary>
        /// Shortcut for mock.Setup(...), creating the mock when necessary.
        /// For specific return types. E.g. primitive, structs
        /// that cannot be infered
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="setup"></param>
        /// <returns></returns>
        public ISetup<TService, TReturn> Setup<TService, TReturn>(Expression<Func<TService, TReturn>> setup)
            where TService : class
        {
            return Setup<ISetup<TService, TReturn>, TService>(m => m.Setup(setup));
        }

        private TReturn Setup<TReturn, TService>(Func<Mock<TService>, TReturn> returnValue)
            where TService : class
        {
            var mock = (Mock<TService>) GetOrMakeMockFor(typeof (TService));
            Use(mock);
            return returnValue(mock);
        }

        /// <summary>
        /// Shortcut for mock.SetupAllProperties(), creating the mock when necessary
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public Mock<TService> SetupAllProperties<TService>() where TService : class
        {
            var mock = (Mock<TService>)GetOrMakeMockFor(typeof(TService));
            Use(mock);
            mock.SetupAllProperties();
            return mock;
        }


        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to 
        /// other interfaces. In the end, this just means that all given
        /// types will be implemnted by the same instance.
        /// </summary>
        public void Combine(Type type, params Type[] forwardTo)
        {
            var mockObject = new MockInstance(type);
            forwardTo.Aggregate(mockObject.Mock, As);

            foreach (var serviceType in forwardTo.Concat(new[] { type }))
                typeMap[serviceType] = mockObject;
        }

        private static Mock As(Mock mock, Type forInterface)
        {
            var genericMethodDef = mock.GetType().GetMethods().Where(x => x.Name == "As");
            var method = genericMethodDef.First().MakeGenericMethod(forInterface);
            return (Mock) method.Invoke(mock, null);
        }

        /*extra verify methods to copy with primitive return type
         * Not using the TextTempatingFileGenerator for now for this
         */
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression);
        }


        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times);
        }


        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Func<Times> times)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            
            mock.Verify(expression, times);
        }


        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, String failMessage)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, failMessage);
        }


        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times, String failMessage)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times, failMessage);
        }

    }
}