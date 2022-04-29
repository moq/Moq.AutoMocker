using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeWith
{
    [TestMethod]
    public void You_can_register_a_concrete_instance_by_its_iterface()
    {
        AutoMocker mocker = new();

        mocker.With<IService2, Service2>();

        Assert.IsInstanceOfType(mocker.Get<IService2>(), typeof(Service2));
    }

    [TestMethod]
    public void You_can_register_a_concrete_instance()
    {
        AutoMocker mocker = new();

        mocker.With<Service2>();

        Assert.IsInstanceOfType(mocker.Get<Service2>(), typeof(Service2));
    }

    [TestMethod]
    public void You_can_register_a_concrete_instance_by_its_iterface_non_generic()
    {
        AutoMocker mocker = new();

        mocker.With(typeof(IService2), typeof(Service2));

        Assert.IsInstanceOfType(mocker.Get<IService2>(), typeof(Service2));
    }

    [TestMethod]
    public void You_can_register_a_concrete_instance_non_generic()
    {
        AutoMocker mocker = new();

        mocker.With(typeof(Service2));

        Assert.IsInstanceOfType(mocker.Get<Service2>(), typeof(Service2));
    }
}

