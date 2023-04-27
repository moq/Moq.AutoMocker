using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeUsingExplicitObjects
{
    [TestMethod]
    public void You_can_Use_an_instance_as_an_argument_to_GetInstance()
    {
        var mocker = new AutoMocker();
        var empty = new Empty();
        mocker.Use(empty);
        var instance = mocker.CreateInstance<OneConstructor>();
        Assert.AreSame(empty, instance.Empty);
    }

    [TestMethod]
    public void You_can_use_Use_as_an_alias_for_MockOf()
    {
        var mocker = new AutoMocker();
        mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
        var instance = mocker.CreateInstance<WithService>();
        Assert.IsInstanceOfType(instance.Service, typeof(IService2));
        Assert.IsInstanceOfType(instance.Service.Other, typeof(IService1));
    }

    [TestMethod]
    public void Adding_an_instance_will_replace_existing_setups()
    {
        var mocker = new AutoMocker();
        mocker.Use<IService2>(x => x.Other.ToString() == "kittens");
        var otherService = Mock.Of<IService2>();
        mocker.Use(otherService);
        Assert.AreSame(otherService, mocker.Get<IService2>());
    }

    [TestMethod]
    public void It_throws_if_type_is_null()
    {
        AutoMocker mocker = new();

        var ex = Assert.ThrowsException<ArgumentNullException>(() => mocker.Use(null!, new object()));
        Assert.AreEqual("type", ex.ParamName);
    }

    [TestMethod]
    public void It_throws_if_service_is_not_of_the_specified_type()
    {
        AutoMocker mocker = new();

        var ex = Assert.ThrowsException<ArgumentException>(() => mocker.Use(typeof(int), new object()));
        Assert.AreEqual("service", ex.ParamName);
    }

    [TestMethod]
    public void It_throws_if_cache_is_not_registered()
    {
        AutoMocker mocker = new();
        mocker.Resolvers.Remove(mocker.Resolvers.OfType<CacheResolver>().Single());

        Assert.ThrowsException<InvalidOperationException>(() => mocker.Use(typeof(object), new object()));
    }

    [TestMethod]
    public void It_throws_if_the_service_has_already_been_added()
    {
        AutoMocker mocker = new();
        Service2 service = new();
        mocker.Use(service);
        var ex = Assert.ThrowsException<InvalidOperationException>(() => mocker.Use(service));
        Assert.AreEqual("The service has already been added.", ex.Message);
    }

    [TestMethod]
    public void It_throws_if_the_service_was_created_by_mocker()
    {
        AutoMocker mocker = new();
        Mock<IService1> service = mocker.GetMock<IService1>();
        var ex = Assert.ThrowsException<InvalidOperationException>(() => mocker.Use(service));
        Assert.AreEqual("The service has already been added.", ex.Message);
    }

    [TestMethod]
    public void It_replaces_previous_service_with_specified_service()
    {
        AutoMocker mocker = new();
        Service2 service1 = new();
        Service2 service2 = new();

        mocker.Use(service1);
        mocker.Use(service2);
        
        Assert.AreEqual(service2, mocker.Get<Service2>());
    }

    [TestMethod]
    public void It_replaces_previous_mock_service_with_specified_service()
    {
        AutoMocker mocker = new();
        //Creates and caches a mock of IService2
        _ = mocker.GetMock<IService2>();
        Service2 service2 = new();

        mocker.Use<IService2>(service2);

        Assert.AreEqual(service2, mocker.Get<IService2>());
    }

    [TestMethod]
    public void It_replaces_previous_mock_service_with_specified_mock_service()
    {
        AutoMocker mocker = new();
        //Creates and caches a mock of IService2
        _ = mocker.GetMock<IService2>();
        
        Mock<IService2> service2 = new();

        mocker.Use(service2);

        Assert.AreEqual(service2.Object, mocker.Get<IService2>());
    }
}
