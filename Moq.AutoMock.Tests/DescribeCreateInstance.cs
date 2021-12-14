using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeCreateInstance
{
    [TestMethod]
    public void It_creates_object_with_no_constructor()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<Empty>();
        Assert.IsNotNull(instance);
    }

    [TestMethod]
    public void It_creates_objects_for_ctor_parameters()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<OneConstructor>();
        Assert.IsNotNull(instance.Empty);
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_parameters()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<OneConstructor>();
        Assert.IsNotNull(Mock.Get(instance.Empty));
    }

    [TestMethod]
    public void It_creates_mock_objects_for_internal_ctor_parameters()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<WithServiceInternal>(true);
        Assert.IsNotNull(Mock.Get(instance.Service));
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_parameters_with_supplied_behavior()
    {
        var strictMocker = new AutoMocker(MockBehavior.Strict);

        var instance = strictMocker.CreateInstance<OneConstructor>();
        var mock = Mock.Get(instance.Empty);
        Assert.IsNotNull(mock);
        Assert.AreEqual(MockBehavior.Strict, mock.Behavior);
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_sealed_parameters_when_instances_provided()
    {
        var mocker = new AutoMocker();
        mocker.Use("Hello World");
        WithSealedParams instance = mocker.CreateInstance<WithSealedParams>();
        Assert.AreEqual("Hello World", instance.Sealed);
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_array_parameters()
    {
        var mocker = new AutoMocker();
        WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
        IService2[] services = instance.Services;
        Assert.IsNotNull(services);
        Assert.AreEqual(1, services.Length);
        Assert.IsTrue(services[0] is IService2);
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_array_parameters_with_elements()
    {
        var mocker = new AutoMocker();
        var expectedService = new Mock<IService2>();
        mocker.Use(expectedService);
        WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
        IService2[] services = instance.Services;
        Assert.IsNotNull(services);
        Assert.AreEqual(1, services.Length);
        Assert.AreEqual(expectedService.Object, services[0]);
    }

    [TestMethod]
    public void It_throws_original_exception_caught_whilst_creating_object()
    {
        var mocker = new AutoMocker();
        Assert.ThrowsException<ArgumentException>(mocker.CreateInstance<ConstructorThrows>);
    }

    [TestMethod]
    public void It_throws_original_exception_caught_whilst_creating_object_with_original_stack_trace()
    {
        var mocker = new AutoMocker();
        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() => mocker.CreateInstance<ConstructorThrows>());
        StringAssert.Contains(exception.StackTrace, typeof(ConstructorThrows).Name);
    }

    [TestMethod]
    public void It_creates_object_when_first_level_dependencies_are_classes()
    {
        var mocker = new AutoMocker();
        HasClassDependency instance = mocker.CreateInstance<HasClassDependency>();
        var dependency = instance.WithService;
        Assert.IsNotNull(dependency);
        Assert.IsInstanceOfType(dependency, typeof(WithService));
        Assert.IsInstanceOfType(Mock.Get(dependency), typeof(Mock<WithService>));
        Assert.AreSame(dependency, mocker.Get<WithService>());
    }

    [TestMethod]
    public void It_creates_object_with_2_first_level_dependencies()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<With2ClassDependencies>();

        var dependency1 = instance.WithService;
        Assert.IsNotNull(dependency1);
        Assert.IsInstanceOfType(dependency1, typeof(WithService));
        Assert.IsInstanceOfType(Mock.Get(dependency1), typeof(Mock<WithService>));
        Assert.AreSame(dependency1, mocker.Get<WithService>());

        var dependency2 = instance.With3Parameters;
        Assert.IsNotNull(dependency2);
        Assert.IsInstanceOfType(dependency2, typeof(With3Parameters));
        Assert.IsInstanceOfType(Mock.Get(dependency2), typeof(Mock<With3Parameters>));
    }

    [TestMethod]
    public void Second_level_dependencies_act_same_as_if_they_were_target()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<HasFuncDependencies>();
        var dependency = instance.WithServiceFactory();
        Assert.IsNotNull(dependency);
        Assert.IsInstanceOfType(dependency, typeof(WithService));
        Assert.IsInstanceOfType(Mock.Get(dependency), typeof(Mock<WithService>));
        // Questionable if this is the correct behavior, but it is the current behavior.
        Assert.AreSame(dependency, mocker.Get<WithService>());
    }

    [TestMethod]
    public void It_throws_when_creating_object_with_recursive_dependency()
    {
        var mocker = new AutoMocker();
        // I could see this changing to something else in the future, like null. Right now, it seems
        // best to cause early failure to clarify what went wrong. Also, returning null "allows" the
        // behavior, so it's easier to move that direction later without breaking backward compatibility.
        ArgumentException e = Assert.ThrowsException<ArgumentException>(mocker.CreateInstance<WithRecursiveDependency>);
        Assert.IsTrue(e.Message.StartsWith($"Did not find a best constructor for `{typeof(WithRecursiveDependency)}`"));
    }
}
