using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;
using VerifyMSTest;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeExtractingStrictObjects : VerifyBase
    {
        [TestMethod]
        public void It_creates_a_mock_as_strict_if_the_object_is_missing()
        {
            var mocker = new AutoMocker(MockBehavior.Strict);
            var mock = mocker.GetMock<IService1>();
            Assert.AreEqual(MockBehavior.Strict, mock.Behavior);
        }
    }
}
