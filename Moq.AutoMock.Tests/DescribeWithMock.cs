namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeWithMock
{
    [TestMethod]
    public void It_registers_a_fresh_configurable_mock()
    {
        AutoMocker mocker = new();
        Mock<IService2> original = mocker.GetMock<IService2>();

        Mock<IService2> mock = mocker.WithMock<IService2>();
        mock.Setup(x => x.Name).Returns("configured").Verifiable();

        Assert.AreNotSame(original, mock);
        Assert.AreSame(mock, mocker.GetMock<IService2>());
        Assert.AreSame(mock.Object, mocker.Get<IService2>());
        WithService instance = mocker.CreateInstance<WithService>();
        Assert.AreSame(mock.Object, instance.Service);
        Assert.AreEqual("configured", instance.Service.Name);
        mocker.VerifyAll();
    }

    [TestMethod]
    public void It_registers_a_fresh_configurable_mock_non_generic()
    {
        AutoMocker mocker = new();
        Mock<IService2> original = mocker.GetMock<IService2>();

        Mock<IService2> mock = (Mock<IService2>)mocker.WithMock(typeof(IService2));
        mock.Setup(x => x.Name).Returns("configured").Verifiable();

        Assert.AreNotSame(original, mock);
        Assert.AreSame(mock, mocker.GetMock<IService2>());
        Assert.AreSame(mock.Object, mocker.Get<IService2>());
        WithService instance = mocker.CreateInstance<WithService>();
        Assert.AreSame(mock.Object, instance.Service);
        Assert.AreEqual("configured", instance.Service.Name);
        mocker.VerifyAll();
    }

    [TestMethod]
    public void It_replaces_real_instances_and_creates_a_new_mock_on_each_call()
    {
        AutoMocker mocker = new();
        mocker.Use<IService2>(new Service2());

        Mock<IService2> first = mocker.WithMock<IService2>();
        Mock<IService2> second = mocker.WithMock<IService2>();

        Assert.AreNotSame(first, second);
        Assert.AreSame(second, mocker.GetMock<IService2>());
    }

    [TestMethod]
    public void It_resolves_class_constructor_dependencies()
    {
        AutoMocker mocker = new();
        IService2 dependency = mocker.Get<IService2>();

        Mock<WithService> mock = mocker.WithMock<WithService>();

        Assert.AreSame(dependency, mock.Object.Service);
        Assert.AreSame(mock.Object, mocker.Get<WithService>());
        Assert.AreSame(mock, mocker.GetMock<WithService>());
    }

    [TestMethod]
    public void It_resolves_class_constructor_dependencies_non_generic()
    {
        AutoMocker mocker = new();
        IService2 dependency = mocker.Get<IService2>();

        Mock<WithService> mock = (Mock<WithService>)mocker.WithMock(typeof(WithService));

        Assert.AreSame(dependency, mock.Object.Service);
        Assert.AreSame(mock.Object, mocker.Get<WithService>());
        Assert.AreSame(mock, mocker.GetMock<WithService>());
    }

    [TestMethod]
    public void It_inherits_and_can_override_container_settings()
    {
        CustomDefaultValueProvider provider = new();
        AutoMocker mocker = new(MockBehavior.Strict, DefaultValue.Custom, provider, true);

        Mock<IService2> inherited = mocker.WithMock<IService2>();
        Assert.AreEqual(MockBehavior.Strict, inherited.Behavior);
        Assert.AreSame(provider, inherited.DefaultValueProvider);
        Assert.IsTrue(inherited.CallBase);

        Mock<IService2> overridden = mocker.WithMock<IService2>(
            mockBehavior: MockBehavior.Loose, defaultValue: DefaultValue.Mock, callBase: false);
        Assert.AreEqual(MockBehavior.Loose, overridden.Behavior);
        Assert.AreEqual(DefaultValue.Mock, overridden.DefaultValue);
        Assert.IsFalse(overridden.CallBase);
    }

    [TestMethod]
    public void It_inherits_and_can_override_container_settings_non_generic()
    {
        CustomDefaultValueProvider provider = new();
        AutoMocker mocker = new(MockBehavior.Strict, DefaultValue.Custom, provider, true);

        Mock inherited = mocker.WithMock(typeof(IService2));
        Assert.AreEqual(MockBehavior.Strict, inherited.Behavior);
        Assert.AreSame(provider, inherited.DefaultValueProvider);
        Assert.IsTrue(inherited.CallBase);

        Mock overridden = mocker.WithMock(typeof(IService2),
            mockBehavior: MockBehavior.Loose, defaultValue: DefaultValue.Mock, callBase: false);
        Assert.AreEqual(MockBehavior.Loose, overridden.Behavior);
        Assert.AreEqual(DefaultValue.Mock, overridden.DefaultValue);
        Assert.IsFalse(overridden.CallBase);
    }

    [TestMethod]
    public void It_validates_runtime_types()
    {
        AutoMocker mocker = new();

        Assert.ThrowsExactly<ArgumentNullException>(() => mocker.WithMock(null!));
        Assert.ThrowsExactly<InvalidOperationException>(() => mocker.WithMock(typeof(string)));
    }
}
