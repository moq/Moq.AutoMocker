﻿using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Moq.AutoMock
{
    /// <summary>
    /// An auto-mocking IoC container that generates mock objects using Moq.
    /// </summary>
    public partial class AutoMocker
    {
        private readonly Dictionary<Type, IInstance> typeMap = new Dictionary<Type, IInstance>();
        private readonly MockRepositoryShim mockRepository;
        private readonly MethodInfo mockRepositoryCreateMethod;

        public AutoMocker()
            : this(MockBehavior.Default)
        {
        }

        public AutoMocker(MockBehavior mockBehavior)
            : this(mockBehavior, DefaultValue.Empty)
        {
        }

        public AutoMocker(MockBehavior mockBehavior, DefaultValue defaultValue)
            : this(mockBehavior, defaultValue, callBase: false)
        {
        }

        public AutoMocker(MockBehavior mockBehavior, DefaultValue defaultValue, bool callBase)
        {
            mockRepository = new MockRepositoryShim(mockBehavior)
            {
                DefaultValue = defaultValue,
                CallBase = callBase
            };
            mockRepositoryCreateMethod = mockRepository.GetType().GetMethod("Create", new Type[] { });
        }

        public MockBehavior MockBehavior => this.mockRepository.MockBehavior;
        public DefaultValue DefaultValue => this.mockRepository.DefaultValue;
        public bool CallBase => this.mockRepository.CallBase;

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
            return CreateInstance<T>(false);
        }

        /// <summary>
        /// Constructs an instance from known services. Any dependancies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <typeparam name="T">A concrete type</typeparam>
        /// <param name="enablePrivate">When true, private constructors will also be used to
        /// create mocks.</param>
        /// <returns>An instance of T with all constructor arguments derrived from services 
        /// setup in the container.</returns>
        public T CreateInstance<T>(bool enablePrivate) where T : class
        {
            var bindingFlags = GetBindingFlags(enablePrivate);
            var arguments = CreateArguments<T>(bindingFlags);
            try
            {
                var ctor = ConstructorSelector.SelectFor(typeof(T), typeMap.Keys.ToArray(), bindingFlags);
                return (T) ctor.Invoke(arguments);
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw;  //Not really reachable either way, but I like this better than return default(T)
            }
        }

        private static BindingFlags GetBindingFlags(bool enablePrivate)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (enablePrivate) bindingFlags = bindingFlags | BindingFlags.NonPublic;
            return bindingFlags;
        }

        private object[] CreateArguments<T>(BindingFlags bindingFlags) where T : class
        {
            var ctor = ConstructorSelector.SelectFor(typeof(T), typeMap.Keys.ToArray(), bindingFlags);
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
            return CreateSelfMock<T>(false);
        }

        /// <summary>
        /// Constructs a self-mock from the services available in the container. A self-mock is
        /// a concrete object that has virtual and abstract members mocked. The purpose is so that
        /// you can test the majority of a class but mock out a resource. This is great for testing
        /// abstract classes, or avoiding breaking cohesion even further with a non-abstract class.
        /// </summary>
        /// <typeparam name="T">The instance that you want to build</typeparam>
        /// <param name="enablePrivate">When true, private constructors will also be used to
        /// create mocks.</param>
        /// <returns>An instance with virtual and abstract members mocked</returns>
        public T CreateSelfMock<T>(bool enablePrivate) where T : class
        {
            var arguments = CreateArguments<T>(GetBindingFlags(enablePrivate));
            var mock = mockRepository.Create<T>(arguments);
            typeMap.Add(typeof(T), new MockInstance(mock));
            return mock.Object;
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
                typeMap[type] = new MockInstance(CreateMockOf(type));
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
            return typeMap[type] = new MockInstance(CreateMockOf(type));
        }

        private Mock CreateMockOf(Type type)
        {
            MethodInfo generic = mockRepositoryCreateMethod.MakeGenericMethod(type);
            var mock = (Mock)generic.Invoke(mockRepository, null);
            return mock;
        }

        /// <summary>
        /// Adds an intance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="service"></param>
        public void Use<TService>(TService service) => Use(typeof(TService), service);

        /// <summary>
        /// Adds an intance to the container.
        /// </summary>
        /// <param name="type">The type of service to use</param>
        /// <param name="service">The service to use</param>
        public void Use(Type type, object service)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (service != null && !type.IsInstanceOfType(service))
                throw new ArgumentException($"{nameof(service)} is not of type {type}");
            Mock serviceMock = service as Mock;
            if (serviceMock != null)
            {
                mockRepository.Add(serviceMock);
            }
            typeMap[type] = new RealInstance(service);
        }

        /// <summary>
        /// Adds an intance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="mockedService">The mocked service</param>
        public void Use<TService>(Mock<TService> mockedService)
            where TService : class
        {
            mockRepository.Add(mockedService);
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
        public TService Get<TService>() => GetImplementation<TService>(typeof(TService));

        /// <summary>
        /// Searches and retrieves an object from the container that matches the serviceType. This can be
        /// a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve</param>
        /// <returns></returns>
        public object Get(Type serviceType) => GetImplementation<object>(serviceType);

        /// <summary>
        /// Searches and retrieves the mock that the container uses for TService.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <exception cref="ArgumentException">if the requested object wasn't a Mock</exception>
        /// <returns>A mock of TService</returns>
        public Mock<TService> GetMock<TService>() where TService : class
        {
            return GetMockImplementation<TService>(typeof(TService));
        }

        /// <summary>
        /// Searches and retrieves the mock that the container uses for serviceType.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve</param>
        /// <returns>A mock of serviceType</returns>
        public Mock<object> GetMock(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (!serviceType.GetTypeInfo().IsClass) throw new ArgumentException($"'{serviceType.FullName}' is not a class", nameof(serviceType));

            return GetMockImplementation<object>(serviceType);
        }

        /// <summary>
        /// This is a shortcut for calling `mock.VerifyAll()` on every mock that we have.
        /// </summary>
        public void VerifyAll()
        {
            mockRepository.VerifyAll();
        }

        /// <summary>
        /// This is a shortcut for calling `mock.Verify()` on every mock that we have.
        /// </summary>
        public void Verify()
        {
            mockRepository.Verify();
        }

        /// <summary>
        /// Shortcut for mock.Setup(...), creating the mock when necessary.
        /// </summary>
        [Obsolete("No longer supported with Moq 4.8 and later. Use Setup<TService, TReturn> instead.", true)]
        public ISetup<TService, object> Setup<TService>(Expression<Func<TService, object>> setup)
            where TService : class
        {
            throw new NotSupportedException("No longer supported in Moq 4.8 and later. Use Setup<TService, TReturn> instead.");
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
            var mockObject = new MockInstance(CreateMockOf(type));
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

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        /// <param name="times"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        /// <param name="times"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Func<Times> times)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            
            mock.Verify(expression, times);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        /// <param name="failMessage"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, String failMessage)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, failMessage);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        /// <param name="times"></param>
        /// <param name="failMessage"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times, String failMessage)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times, failMessage);
        }

        private Mock<T> GetMockImplementation<T>(Type serviceType) where T : class
        {
            if (!typeMap.TryGetValue(serviceType, out var instance))
                instance = CreateMockObjectAndStore(serviceType);

            if (!instance.IsMock)
                throw new ArgumentException(string.Format("Registered service `{0}` was not a mock", GetImplementation<object>(serviceType).GetType()));

            var mockInstance = (MockInstance) instance;
            return (Mock<T>) mockInstance.Mock;
        }

        private T GetImplementation<T>(Type serviceType)
        {
            if (!typeMap.TryGetValue(serviceType, out _))
                CreateMockObjectAndStore(serviceType);

            return (T) typeMap[serviceType].Value;
        }
    }
}
