using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using Moq.AutoMock.Tests.Util;
using System;
using System.Collections.Generic;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeResolvedObjects
    {
        [TestMethod]
        public void ResolvedObjects_includes_manually_registered_services()
        {
            AutoMocker mocker = new();
            Service2 service = new();
            mocker.Use<IService2>(service);

            IReadOnlyDictionary<Type, object?> resolved = mocker.ResolvedObjects;

            Assert.AreEqual(1, resolved.Count);
            Assert.AreEqual(service, resolved[typeof(IService2)]);
        }

        [TestMethod]
        public void ResolvedObjects_includes_created_mock_objects()
        {
            AutoMocker mocker = new();
            _ = mocker.CreateInstance<WithService>();

            IReadOnlyDictionary<Type, object?> resolved = mocker.ResolvedObjects;

            Assert.AreEqual(1, resolved.Count);
            Assert.IsTrue(resolved[typeof(IService2)] is Mock<IService2>);
        }

        [TestMethod]
        public void ResolvedObjects_includes_created_primitive_arrays()
        {
            AutoMocker mocker = new();
            _ = mocker.CreateInstance<WithArrayParameter>();

            IReadOnlyDictionary<Type, object?> resolved = mocker.ResolvedObjects;

            Assert.AreEqual(1, resolved.Count);
            var resolvedArray = resolved[typeof(string[])] as string[];
            Assert.AreEqual(0, resolvedArray?.Length);
        }

        [TestMethod]
        public void ResolvedObjects_includes_created_service_arrays()
        {
            AutoMocker mocker = new();
            var service = new Service2();
            mocker.Use<IService2>(service);
            _ = mocker.CreateInstance<WithServiceArray>();

            IReadOnlyDictionary<Type, object?> resolved = mocker.ResolvedObjects;

            Assert.AreEqual(2, resolved.Count);
            var resolvedArray = resolved[typeof(IService2[])] as IService2[];
            Assert.AreEqual(1, resolvedArray?.Length);
            Assert.AreEqual(service, resolvedArray![0]);
        }

        [TestMethod]
        public void ResolvedObjects_custom_resolver_providing_value_prevents_subsequent_resolver_from_being_invoked()
        {
            AutoMocker mocker = new();
            object singleton = new();
            mocker.Resolvers.Clear();
            mocker.Resolvers.Add(new SingletonResolver<object>(singleton));
            mocker.Resolvers.Add(new ThrowingResolver());

            object resolved = mocker.Get<object>();
            Assert.AreEqual(singleton, resolved);
        }

        [TestMethod]
        public void ResolvedObjects_custom_resolver_can_prempt_cache_resolver()
        {
            object singleton = new();
            object used = new();
            AutoMocker mocker = new();
            mocker.Use(used);
            mocker.Resolvers.Insert(0, new SingletonResolver<object>(singleton));
            
            object resolved = mocker.Get<object>();
            Assert.AreEqual(singleton, resolved);
        }

        private class ThrowingResolver : IMockResolver
        {
            public void Resolve(MockResolutionContext context) 
                => throw new NotImplementedException();
        }

        private class SingletonResolver<T> : IMockResolver
        {
            public SingletonResolver(T value)
            {
                Value = value;
            }

            public T Value { get; }

            public void Resolve(MockResolutionContext context)
            {
                if (context.RequestType == typeof(T))
                {
                    context.Value = Value;
                }
            }
        }
    }
}
