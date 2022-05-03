namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeWithSelfMock
{
    [TestMethod]
    [Description("Issue 144")]
    public void It_can_register_a_custom_default_value_provider_for_a_self_mock_with_interface()
    {
        AutoMocker mocker = new();
        CustomDefaultValueProvider provider = new();

        var mock = mocker.WithSelfMock<IService2, Service2>(defaultValue: DefaultValue.Custom, defaultValueProvider: provider);

        Assert.AreEqual(provider, Mock.Get(mock).DefaultValueProvider);
    }

    [TestMethod]
    [Description("Issue 144")]
    public void It_uses_default_value_provider_for_a_self_mock_with_interface()
    {
        CustomDefaultValueProvider provider = new();
        AutoMocker mocker = new(MockBehavior.Default, DefaultValue.Custom, provider, false);

        var mock = mocker.WithSelfMock<IService2, Service2>();

        Assert.AreEqual(provider, Mock.Get(mock).DefaultValueProvider);
    }

    [TestMethod]
    [Description("Issue 144")]
    public void It_can_register_a_custom_default_value_provider_for_a_self_mock_with_class()
    {
        AutoMocker mocker = new();
        CustomDefaultValueProvider provider = new();

        var mock = mocker.WithSelfMock<Service2>(defaultValue: DefaultValue.Custom, defaultValueProvider: provider);

        Assert.AreEqual(provider, Mock.Get(mock).DefaultValueProvider);
    }

    [TestMethod]
    [Description("Issue 144")]
    public void It_uses_default_value_provider_for_a_self_mock_with_class()
    {
        CustomDefaultValueProvider provider = new();
        AutoMocker mocker = new(MockBehavior.Default, DefaultValue.Custom, provider, false);

        var mock = mocker.WithSelfMock<Service2>();

        Assert.AreEqual(provider, Mock.Get(mock).DefaultValueProvider);
    }

    [TestMethod]
    [Description("Issue 144")]
    public void It_can_register_a_custom_default_value_provider_for_a_self_mock_with_interface_type()
    {
        AutoMocker mocker = new();
        CustomDefaultValueProvider provider = new();

        var mock = (Service2)mocker.WithSelfMock(typeof(IService2), typeof(Service2), defaultValue: DefaultValue.Custom, defaultValueProvider: provider);

        Assert.AreEqual(provider, Mock.Get(mock).DefaultValueProvider);
    }

    [TestMethod]
    [Description("Issue 144")]
    public void It_uses_default_value_provider_for_a_self_mock_with_interface_type()
    {
        CustomDefaultValueProvider provider = new();
        AutoMocker mocker = new(MockBehavior.Default, DefaultValue.Custom, provider, false);

        var mock = (Service2)mocker.WithSelfMock(typeof(IService2), typeof(Service2));

        Assert.AreEqual(provider, Mock.Get(mock).DefaultValueProvider);
    }

    [TestMethod]
    [Description("Issue 144")]
    public void It_can_register_a_custom_default_value_provider_for_a_self_mock_with_class_type()
    {
        AutoMocker mocker = new();
        CustomDefaultValueProvider provider = new();

        var mock = (Service2)mocker.WithSelfMock(typeof(Service2), defaultValue: DefaultValue.Custom, defaultValueProvider: provider);

        Assert.AreEqual(provider, Mock.Get(mock).DefaultValueProvider);
    }

    [TestMethod]
    [Description("Issue 144")]
    public void It_uses_default_value_provider_for_a_self_mock_with_class_type()
    {
        CustomDefaultValueProvider provider = new();
        AutoMocker mocker = new(MockBehavior.Default, DefaultValue.Custom, provider, false);

        var mock = (Service2)mocker.WithSelfMock(typeof(Service2));

        Assert.AreEqual(provider, Mock.Get(mock).DefaultValueProvider);
    }
}

