using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeWith
{
    [TestMethod]
    public void You_can_register_a_concrete_instance_by_its_interface()
    {
        AutoMocker mocker = new();

        Service2 instance = mocker.With<IService2, Service2>();

        IService2 retrieved = mocker.Get<IService2>();
        Assert.IsInstanceOfType(retrieved, typeof(Service2));
        Assert.AreSame(instance, retrieved);
    }

    [TestMethod]
    public void You_can_register_a_concrete_instance()
    {
        AutoMocker mocker = new();

        Service2 instance = mocker.With<Service2>();

        Service2 retrieved = mocker.Get<Service2>();
        Assert.AreSame(instance, retrieved);
    }

    [TestMethod]
    public void You_can_register_a_concrete_instance_by_its_interface_non_generic()
    {
        AutoMocker mocker = new();

        object instance = mocker.With(typeof(IService2), typeof(Service2));

        IService2 retrieved = mocker.Get<IService2>();
        Assert.IsInstanceOfType(retrieved, typeof(Service2));
        Assert.IsInstanceOfType(instance, typeof(Service2));
        Assert.AreSame(instance, retrieved);
    }

    [TestMethod]
    public void You_can_register_a_concrete_instance_non_generic()
    {
        AutoMocker mocker = new();

        object instance = mocker.With(typeof(Service2));

        Service2 retrieved = mocker.Get<Service2>();
        Assert.IsInstanceOfType(retrieved, typeof(Service2));
        Assert.IsInstanceOfType(instance, typeof(Service2));
        Assert.AreSame(instance, retrieved);
    }
}

