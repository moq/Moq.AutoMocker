using Moq.AutoMock.Resolvers;
using Moq.Language;
using Moq.Language.Flow;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
#if NET45
using Array = Moq.AutoMock.Resolvers.Array;
#endif

namespace Moq.AutoMock
{
    /// <summary>
    /// An auto-mocking IoC container that generates mock objects using Moq.
    /// </summary>
    public partial class AutoMocker
    {
        private readonly Dictionary<Type, IInstance> _typeMap = new Dictionary<Type, IInstance>();

        /// <summary>
        /// Initializes an instance of AutoMockers.
        /// </summary>
        public AutoMocker()
            : this(MockBehavior.Default)
        {
        }

        /// <summary>
        /// Initializes an instance of AutoMockers.
        /// </summary>
        /// <param name="mockBehavior">The behavior to use for created mocks.</param>
        public AutoMocker(MockBehavior mockBehavior)
            : this(mockBehavior, DefaultValue.Empty)
        {
        }

        /// <summary>
        /// Initializes an instance of AutoMockers.
        /// </summary>
        /// <param name="mockBehavior">The behavior to use for created mocks.</param>
        /// <param name="defaultValue">The default value to use for created mocks.</param>
        public AutoMocker(MockBehavior mockBehavior, DefaultValue defaultValue)
            : this(mockBehavior, defaultValue, callBase: false)
        {
        }

        /// <summary>
        /// Initializes an instance of AutoMockers.
        /// </summary>
        /// <param name="mockBehavior">The behavior to use for created mocks.</param>
        /// <param name="defaultValue">The default value to use for created mocks.</param>
        /// <param name="callBase">Whether to call the base virtual implementation for created mocks.</param>
        public AutoMocker(MockBehavior mockBehavior, DefaultValue defaultValue, bool callBase)
        {
            MockBehavior = mockBehavior;
            DefaultValue = defaultValue;
            CallBase = callBase;

            Resolvers = new List<IMockResolver>
            {
                new MockResolver(mockBehavior, defaultValue, callBase),
                new FuncResolver(),
                new LazyResolver(),
                new EnumerableResolver()
            };
        }

        /// <summary>
        /// Behavior of created mocks, according to the value set in the constructor.
        /// </summary>
        public MockBehavior MockBehavior { get; }

        /// <summary>
        /// Specifies the behavior to use when returning default values for 
        /// unexpected invocations on loose mocks created by this instance.
        /// </summary>
        public DefaultValue DefaultValue { get; }
        
        /// <summary>
        /// Whether the base member virtual implementation will be called 
        /// for created mocks if no setup is matched. Defaults to <c>false</c>.
        /// </summary>
        public bool CallBase { get; }

        /// <summary>
        /// A collection of resolves determining how a given dependency will be resolved.
        /// </summary>
        public ICollection<IMockResolver> Resolvers { get; }

        /// <summary>
        /// A collection of objects stored in this AutoMocker instance.
        /// The keys are the types used when resolving services.
        /// </summary>
        public IReadOnlyDictionary<Type, object?> ResolvedObjects
            => _typeMap.ToDictionary(kvp => kvp.Key, kvp => {
                return kvp.Value switch
                {
                    MockInstance mockInstance => mockInstance.Mock,
                    _ => kvp.Value.Value
                };
            });

        private IInstance Resolve(Type serviceType, ObjectGraphContext resolutionContext)
        {
            if (serviceType.IsArray)
            {
                Type elmType = serviceType.GetElementType() ?? throw new InvalidOperationException($"Could not determine element type for '{serviceType}'");
                MockArrayInstance instance = new(elmType);
                if (_typeMap.TryGetValue(elmType, out var element))
                    instance.Add(element);
                return instance;
            }

            object? resolved = Resolve(serviceType, null, resolutionContext);
            return resolved switch
            {
                Mock mock => new MockInstance(mock),
                IInstance instance => instance,
                _ => new RealInstance(resolved),
            };
        }

        private object? Resolve(Type serviceType, object? initialValue, ObjectGraphContext resolutionContext)
        {
            if (resolutionContext.VisitedTypes.Contains(serviceType))
            {
                var message = string.Join(Environment.NewLine,
                     $"Class could not be constructed because it appears to be used recursively: '{serviceType}'",
                     string.Join($"{Environment.NewLine} --> ", resolutionContext.VisitedTypes)
                );
                throw new InvalidOperationException(message);
            }

            resolutionContext.VisitedTypes.Add(serviceType);
            var context = new MockResolutionContext(this, serviceType, initialValue, resolutionContext);

            foreach (var r in Resolvers)
                r.Resolve(context);

            return context.Value;
        }

        #region Create Instance/SelfMock

        /// <summary>
        /// Constructs an instance from known services. Any dependencies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <typeparam name="T">A concrete type</typeparam>
        /// <returns>An instance of T with all constructor arguments derived from services 
        /// setup in the container.</returns>
        public T CreateInstance<T>() where T : class
            => CreateInstance<T>(false);

        /// <summary>
        /// Constructs an instance from known services. Any dependencies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <typeparam name="T">A concrete type</typeparam>
        /// <param name="enablePrivate">When true, non-public constructors will also be used to create mocks.</param>
        /// <returns>An instance of T with all constructor arguments derived from services 
        /// setup in the container.</returns>
        public T CreateInstance<T>(bool enablePrivate) where T : class
            => (T)CreateInstance(typeof(T), enablePrivate);

        /// <summary>
        /// Constructs an instance from known services. Any dependencies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <param name="type">A concrete type</param>
        /// <returns>An instance of type with all constructor arguments derived from services 
        /// setup in the container.</returns>
        public object CreateInstance(Type type) => CreateInstance(type, false);

        /// <summary>
        /// Constructs an instance from known services. Any dependencies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <param name="type">A concrete type</param>
        /// <param name="enablePrivate">When true, non-public constructors will also be used to create mocks.</param>
        /// <returns>An instance of type with all constructor arguments derived from services 
        /// setup in the container.</returns>
        public object CreateInstance(Type type, bool enablePrivate)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));

            var context = new ObjectGraphContext(enablePrivate);
            object?[] arguments = CreateArguments(type, context);
            try
            {
                var ctor = type.SelectCtor(_typeMap.Keys.ToArray(), context.BindingFlags);
                return ctor.Invoke(arguments);
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException ?? e).Throw();
                throw;  //Not really reachable either way, but I like this better than return default(T)
            }
        }

        /// <summary>
        /// Constructs a self-mock from the services available in the container. A self-mock is
        /// a concrete object that has virtual and abstract members mocked. The purpose is so that
        /// you can test the majority of a class but mock out a resource. This is great for testing
        /// abstract classes, or avoiding breaking cohesion even further with a non-abstract class.
        /// </summary>
        /// <typeparam name="T">The instance that you want to build</typeparam>
        /// <returns>An instance with virtual and abstract members mocked</returns>
        public T CreateSelfMock<T>() where T : class?
            => CreateSelfMock<T>(false);

        /// <summary>
        /// Constructs a self-mock from the services available in the container. A self-mock is
        /// a concrete object that has virtual and abstract members mocked. The purpose is so that
        /// you can test the majority of a class but mock out a resource. This is great for testing
        /// abstract classes, or avoiding breaking cohesion even further with a non-abstract class.
        /// </summary>
        /// <typeparam name="T">The instance that you want to build</typeparam>
        /// <param name="enablePrivate">When true, non-public constructors will also be used to create mocks.</param>
        /// <returns>An instance with virtual and abstract members mocked</returns>
        public T CreateSelfMock<T>(bool enablePrivate) where T : class?
        {
            var context = new ObjectGraphContext(enablePrivate);
            var arguments = CreateArguments(typeof(T), context);

            var mock = new Mock<T>(MockBehavior, arguments)
            {
                DefaultValue = DefaultValue,
                CallBase = CallBase
            };

            var resolved = Resolve(typeof(T), mock, context);
            if (resolved is Mock<T> m)
                return m.Object;
            
            return default!;
        }

        #endregion Create Instance/SelfMock

        #region Use

        /// <summary>
        /// Adds an instance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="service"></param>
        public void Use<TService>([DisallowNull] TService service)
            => Use(typeof(TService), service ?? throw new ArgumentNullException(nameof(service)));

        /// <summary>
        /// Adds an instance to the container.
        /// </summary>
        /// <param name="type">The type of service to use</param>
        /// <param name="service">The service to use</param>
        public void Use(Type type, object service)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));
            if (service != null && !type.IsInstanceOfType(service))
                throw new ArgumentException($"{nameof(service)} is not of type {type}");
            _typeMap[type] = new RealInstance(service);
        }

        /// <summary>
        /// Adds an instance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="mockedService">The mocked service</param>
        public void Use<TService>(Mock<TService> mockedService)
            where TService : class
        {
            _typeMap[typeof(TService)] = new MockInstance(mockedService ?? throw new ArgumentNullException(nameof(mockedService)));
        }

        /// <summary>
        /// Adds a mock object to the container that implements TService.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="setup">A shortcut for Mock.Of's syntax</param>
        public void Use<TService>(Expression<Func<TService, bool>> setup)
            where TService : class
        {
            if (setup is null) throw new ArgumentNullException(nameof(setup));

            Use(Mock.Get(Mock.Of(setup)));
        }

        #endregion Use

        #region Get

        /// <summary>
        /// Searches and retrieves an object from the container that matches TService. This can be
        /// a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <returns>The object that implements TService</returns>
        public TService Get<TService>() where TService : class?
        {
            if (Get(typeof(TService)) is TService service)
                return service;

            return null!;
        }

        /// <summary>
        /// Searches and retrieves an object from the container that matches TService. This can be
        /// a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <param name="enablePrivate">When true, non-public constructors will also be used to create mocks.</param>
        /// <returns>The object that implements TService</returns>
        public TService Get<TService>(bool enablePrivate) where TService : class?
        {
            if (Get(typeof(TService), enablePrivate) is TService service)
                return service;

            return null!;
        }

        /// <summary>
        /// Searches and retrieves an object from the container that matches the serviceType. This can be
        /// a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve</param>
        /// <returns></returns>
        public object Get(Type serviceType)
        {
            return Get(serviceType, enablePrivate: false);
        }

        /// <summary>
        /// Searches and retrieves an object from the container that matches the serviceType. This can be
        /// a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve</param>
        /// <param name="enablePrivate">When true, non-public constructors will also be used to create mocks.</param>
        /// <returns></returns>
        public object Get(Type serviceType, bool enablePrivate)
        {
            return Get(serviceType, new ObjectGraphContext(enablePrivate));
        }
        
        private object Get(Type serviceType, ObjectGraphContext context)
        {
            if (serviceType is null) throw new ArgumentNullException(nameof(serviceType));

            if (!_typeMap.TryGetValue(serviceType, out var instance) || instance is null)
                instance = _typeMap[serviceType] = Resolve(serviceType, context);

            if (instance is null)
                throw new ArgumentException($"{serviceType} could not resolve to an object.", nameof(serviceType));
            return instance.Value!; //Should generally not be null, unless the caller has forced a null in with Use
        }

        #endregion Get

        #region GetMock

        /// <summary>
        /// Searches and retrieves the mock that the container uses for TService.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <exception cref="ArgumentException">if the requested object wasn't a Mock</exception>
        /// <returns>A mock of TService</returns>
        public Mock<TService> GetMock<TService>() where TService : class
            => (Mock<TService>)GetMock(typeof(TService));

        /// <summary>
        /// Searches and retrieves the mock that the container uses for TService.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <param name="enablePrivate">When true, non-public constructors will also be used to create mocks.</param>
        /// <exception cref="ArgumentException">if the requested object wasn't a Mock</exception>
        /// <returns>A mock of TService</returns>
        public Mock<TService> GetMock<TService>(bool enablePrivate) where TService : class
            => (Mock<TService>)GetMock(typeof(TService), enablePrivate);

        /// <summary>
        /// Searches and retrieves the mock that the container uses for serviceType.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve</param>
        /// <returns>A mock of serviceType</returns>
        public Mock GetMock(Type serviceType)
        {
            if (serviceType is null) throw new ArgumentNullException(nameof(serviceType));

            return GetMock(serviceType, enablePrivate: false);
        }

        /// <summary>
        /// Searches and retrieves the mock that the container uses for serviceType.
        /// </summary>
        /// <param name="serviceType">The type of service to retrieve</param>
        /// <param name="enablePrivate">When true, non-public constructors will also be used to create mocks.</param>
        /// <returns>A mock of serviceType</returns>
        public Mock GetMock(Type serviceType, bool enablePrivate)
        {
            if (serviceType is null) throw new ArgumentNullException(nameof(serviceType));

            return GetMockImplementation(serviceType, enablePrivate);
        }

        private Mock GetMockImplementation(Type serviceType, bool enablePrivate)
        {
            if (!_typeMap.TryGetValue(serviceType, out var instance) || instance is null)
                instance = _typeMap[serviceType] = Resolve(serviceType, new ObjectGraphContext(enablePrivate));

            if (instance == null || !instance.IsMock)
                throw new ArgumentException($"Registered service `{Get(serviceType)?.GetType()}` was not a mock");

            var mockInstance = (MockInstance)instance;
            return mockInstance.Mock;
        }

        #endregion GetMock

        #region Setup

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
        /// that cannot be inferred
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

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a void method.
        /// All parameters are filled with <see cref ="It.IsAny" /> according to the parameter's type.
        /// </summary>
        /// <remarks>
        /// This may only be used on methods that are not overloaded.
        /// This will create the mock when necessary.
        /// </remarks>
        /// <typeparam name="TService">The service type</typeparam>
        /// <param name="methodName">The name of the expected method invocation.</param>
        /// <exception cref="ArgumentNullException">When the methodName is null.</exception>
        /// <exception cref="MissingMethodException">Thrown when no method with methodName is found.</exception>
        /// <exception cref="AmbiguousMatchException">Thrown when more that one method matches the passed method name.</exception>
        /// <returns></returns>
        public ISetup<TService> SetupWithAny<TService>(string methodName)
            where TService : class
        {
            return Setup<ISetup<TService>, TService>(m => m.SetupWithAny(methodName));
        }

        /// <summary>
        /// Specifies a setup on the mocked type for a call to a non-void (value-returning) method.
        /// All parameters are filled with <see cref ="It.IsAny" /> according to the parameter's type.
        /// </summary>
        /// <remarks>
        /// This may only be used on methods that are not overloaded.
        /// This will create the mock when necessary.
        /// </remarks>
        /// <typeparam name="TService">The service type</typeparam>
        /// <typeparam name="TReturn">The return type of the method</typeparam>
        /// <param name="methodName">The name of the expected method invocation.</param>
        /// <exception cref="ArgumentNullException">When the methodName is null.</exception>
        /// <exception cref="MissingMethodException">Thrown when no method with methodName is found.</exception>
        /// <exception cref="AmbiguousMatchException">Thrown when more that one method matches the passed method name.</exception>
        /// <returns></returns>
        public ISetup<TService, TReturn> SetupWithAny<TService, TReturn>(string methodName)
            where TService : class
        {
            return Setup<ISetup<TService, TReturn>, TService>(m => m.SetupWithAny<TService, TReturn>(methodName));
        }

        private TReturn Setup<TReturn, TService>(Func<Mock<TService>, TReturn> returnValue)
            where TService : class
        {
            var mock = (Mock<TService>)GetOrMakeMockFor(typeof(TService));
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
        /// Shortcut for mock.SetupSequence(), creating the mock when necessary
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <returns></returns>
        public ISetupSequentialResult<TReturn> SetupSequence<TService, TReturn>(Expression<Func<TService, TReturn>> setup)
            where TService : class
        {
            if (setup is null) throw new ArgumentNullException(nameof(setup));

            return Setup<ISetupSequentialResult<TReturn>, TService>(m => m.SetupSequence(setup));
        }

        #endregion

        #region Combine

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to 
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine(Type type, params Type[] forwardTo)
        {
            if (type is null) throw new ArgumentNullException(nameof(type));

            Mock mock = forwardTo.Aggregate(GetOrMakeMockFor(type), As);
            foreach (var serviceType in new[] { type }.Concat(forwardTo))
                _typeMap[serviceType] = new MockInstance(mock);

            static Mock As(Mock mock, Type forInterface)
            {
                var method = mock.GetType().GetMethods().First(x => x.Name == nameof(Mock.As))
                    .MakeGenericMethod(forInterface);
                return (Mock)method.Invoke(mock, Array.Empty<object>())!;
            }
        }

        #endregion Combine

        #region Verify

        /// <summary>
        /// This is a shortcut for calling `mock.VerifyAll()` on every mock that we have.
        /// </summary>
        public void VerifyAll()
        {
            foreach (var pair in _typeMap)
            {
                if (pair.Value is MockInstance instance)
                    instance.Mock.VerifyAll();
            }
        }

        /// <summary>
        /// This is a shortcut for calling `mock.Verify()` on every mock that we have.
        /// </summary>
        public void Verify()
        {
            foreach (var pair in _typeMap)
            {
                if (pair.Value is MockInstance instance)
                    instance.Mock.Verify();
            }
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression)
            where T : class
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));

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
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));

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
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));
            if (times is null) throw new ArgumentNullException(nameof(times));

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
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, string failMessage)
            where T : class
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));

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
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times, string failMessage)
            where T : class
        {
            if (expression is null) throw new ArgumentNullException(nameof(expression));
            if (failMessage is null) throw new ArgumentNullException(nameof(failMessage));

            var mock = GetMock<T>();
            mock.Verify(expression, times, failMessage);
        }

        #endregion Verify

        #region Utilities

        internal object?[] CreateArguments(Type type, ObjectGraphContext context)
        {
            ConstructorInfo ctor = type.SelectCtor(_typeMap.Keys.ToArray(), context.BindingFlags);
            if (ctor is null)
                throw new ArgumentException($"`{type}` does not have an acceptable constructor.", nameof(type));

            return ctor.GetParameters().Select(x => Get(x.ParameterType, context)).ToArray();
        }

        private Mock GetOrMakeMockFor(Type type)
        {
            if (!_typeMap.TryGetValue(type, out var instance) || !instance.IsMock)
                instance = Resolve(type, new ObjectGraphContext(false));

            if (instance is not MockInstance mockInstance)
                throw new ArgumentException($"{type} does not resolve to a Mock");

            _typeMap[type] = mockInstance;
            return mockInstance.Mock;
        }

        #endregion
    }
}
