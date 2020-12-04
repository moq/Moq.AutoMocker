using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;
using System;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeSetupAll
    {
        [TestMethod]
        public void You_can_setup_all_on_a_method_with_a_return_value()
        {
            string expected = "SomeValue";
            Mock<IService4> mock = new();

            mock.SetupAll<IService4, string>(nameof(IService4.MainMethodName))
                .Returns(expected);

            string result = mock.Object.MainMethodName("Something");

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void You_can_setup_all_on_a_void_method()
        {
            Mock<IService6> mock = new();

            mock.SetupAll(nameof(IService6.Void))
                .Verifiable();
            
            mock.Object.Void(42, "SomeValue");

            mock.VerifyAll();
        }
    }
}
