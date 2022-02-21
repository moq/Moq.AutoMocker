using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeCreatingSelfMocks
{
    [TestMethod]
    public void Self_mocks_are_useful_for_testing_most_of_class()
    {
        AutoMocker mocker = new();
        var selfMock = mocker.CreateSelfMock<InsecureAboutSelf>();
        selfMock.TellJoke();
        Assert.IsFalse(selfMock.SelfDepricated);
    }

    [TestMethod]
    public void It_can_self_mock_objects_with_constructor_arguments()
    {
        AutoMocker mocker = new();
        var selfMock = mocker.CreateSelfMock<WithService>();
        Assert.IsNotNull(selfMock.Service);
        Assert.IsNotNull(Mock.Get(selfMock.Service));
    }

    [TestMethod]
    [Description("Issue 130")]
    public void It_reuses_dependencies_when_creating_self_mock()
    {
        AutoMocker mocker = new();
        var service = mocker.CreateSelfMock<AbstractService>();
        Assert.IsTrue(ReferenceEquals(service.Dependency, mocker.GetMock<IDependency>().Object));
    }

    [TestMethod]
    [Description("Issue 134")]
    public void It_can_specify_self_mock_with_different_defaults()
    {
        AutoMocker mocker = new(MockBehavior.Loose);
        var fooService = mocker.CreateSelfMock<FooService>(mockBehavior: MockBehavior.Strict, defaultValue: DefaultValue.Mock, callBase: true);

        var mock = Mock.Get(fooService);
        Assert.AreEqual(MockBehavior.Strict, mock.Behavior);
        Assert.AreEqual(DefaultValue.Mock, mock.DefaultValue);
        Assert.AreEqual(true, mock.CallBase);
    }

    [TestMethod]
    [Description("Issue 134")]
    public void It_can_perform_setup_on_interface_of_self_mock_that_is_registered()
    {
        AutoMocker mocker = new();
        var service = mocker.WithSelfMock<IFooService, FooService>();
        mocker.Setup<IFooService, int>(s => s.Foo()).Returns(24);
    }

    [TestMethod]
    [Description("Issue 134")]
    public void It_can_perform_setup_class_of_self_mock_that_is_registered()
    {
        AutoMocker mocker = new();
        var service = mocker.WithSelfMock<FooService>();
        mocker.Setup<FooService, int>(s => s.Foo()).Returns(24);
    }

    [TestMethod]
    [Description("Issue 134")]
    public void It_can_perform_setup_on_interface_of_self_mock_that_is_registered_without_generics()
    {
        AutoMocker mocker = new();
        var service = mocker.WithSelfMock(typeof(IFooService), typeof(FooService));
        mocker.Setup<IFooService, int>(s => s.Foo()).Returns(24);
    }

    [TestMethod]
    [Description("Issue 134")]
    public void It_can_perform_setup_class_of_self_mock_that_is_registered_without_generics()
    {
        AutoMocker mocker = new();
        var service = mocker.WithSelfMock(typeof(FooService));
        mocker.Setup<FooService, int>(s => s.Foo()).Returns(24);
    }

    public abstract class AbstractService
    {
        public IDependency Dependency { get; }
        public AbstractService(IDependency dependency) => Dependency = dependency;
    }

    public interface IDependency { }

    public interface IFooService 
    { 
        int Foo();
    }
    public class FooService : IFooService
    {
        public virtual int Foo() => 42;
    }
}
