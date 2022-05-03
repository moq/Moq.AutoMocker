using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeDefaultValues
{
    [TestMethod]
    [DataRow(DefaultValue.Mock)]
    [DataRow(DefaultValue.Empty)]
    [Description("Issue 144")]
    public void It_creates_mocks_with_default_value_set(DefaultValue defaultValue)
    {
        AutoMocker mocker = new(MockBehavior.Default, defaultValue);

        Mock<IService1> mock = mocker.GetMock<IService1>();

        Assert.AreEqual(defaultValue, mock.DefaultValue);
    }

    [TestMethod]
    [Description("Issue 144")]
    public void It_creates_mocks_with_custom_default_value_resolver()
    {
        CustomDefaultValueProvider defaultValueProvider = new(); 
        AutoMocker mocker = new(MockBehavior.Default, DefaultValue.Custom, defaultValueProvider, false);

        Mock<IService1> mock = mocker.GetMock<IService1>();

        Assert.AreEqual(DefaultValue.Custom, mock.DefaultValue);
        Assert.AreEqual(defaultValueProvider, mock.DefaultValueProvider);
    }

    private class CustomDefaultValueProvider : DefaultValueProvider
    {
        protected override object GetDefaultValue(Type type, Mock mock)
        {
            throw new NotImplementedException();
        }
    }
}
