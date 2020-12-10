using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;
using System;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeSetupWithAny
    {
        [TestMethod]
        public void You_can_setup_all_on_a_method_with_a_return_value()
        {
            string expected = "SomeValue";
            Mock<IService4> mock = new();

            mock.SetupWithAny<IService4, string>(nameof(IService4.MainMethodName))
                .Returns(expected);

            string result = mock.Object.MainMethodName("Something");

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void You_can_setup_all_on_a_void_method()
        {
            Mock<IService6> mock = new();

            mock.SetupWithAny(nameof(IService6.Void))
                .Verifiable();
            
            mock.Object.Void(42, "SomeValue");

            mock.VerifyAll();
        }

        [TestMethod]
        public void When_method_is_not_found_it_throws()
        {
            Mock<IService1> mock = new();

            string expectedMessage = 
                new MissingMethodException(typeof(IService1).Name, "Unknown Method").Message;
            try
            {
                mock.SetupWithAny("Unknown Method");

            }
            catch (MissingMethodException ex) 
                when (ex.Message == expectedMessage)
            { }
        }


    }
}
