using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeGetMock
    {
        [TestMethod]
        public void It_creates_mock_from_interface_from_generic_type_parameter()
        {
            var mocker = new AutoMocker();
            Mock<IService1> mock = mocker.GetMock<IService1>();
            Assert.IsNotNull(mock);
        }

        [TestMethod]
        public void It_creates_mock_from_interface_from_type_parameter()
        {
            var mocker = new AutoMocker();
            Mock<IService1> mock = mocker.GetMock(typeof(IService1)) as Mock<IService1>;
            Assert.IsNotNull(mock);
        }

        [TestMethod]
        public void It_creates_mock_from_class_from_generic_type_parameter()
        {
            var mocker = new AutoMocker();
            Mock<Empty> mock = mocker.GetMock<Empty>();
            Assert.IsNotNull(mock);
        }

        [TestMethod]
        public void It_creates_mock_from_class_from_type_parameter()
        {
            var mocker = new AutoMocker();
            Mock<Empty> mock = mocker.GetMock(typeof(Empty)) as Mock<Empty>;
            Assert.IsNotNull(mock);
        }
    }
}