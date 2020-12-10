using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;
using System;
using System.Reflection;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeSetupWithAny
    {
        [TestMethod]
        public void You_can_setup_with_any_on_a_method_with_a_return_value()
        {
            string expected = "SomeValue";
            Mock<IService4> mock = new();

            mock.SetupWithAny<IService4, string>(nameof(IService4.MainMethodName))
                .Returns(expected);

            string result = mock.Object.MainMethodName("Something");

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void You_can_setup_with_any_on_a_void_method()
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

        [TestMethod]
        [ExpectedException(typeof(AmbiguousMatchException))]
        public void When_void_method_is_overloaded_it_throws()
        {
            Mock<IService7> mock = new();

            mock.SetupWithAny(nameof(IService7.Void));
        }

        [TestMethod]
        [ExpectedException(typeof(AmbiguousMatchException))]
        public void When_method_is_overloaded_it_throws()
        {
            Mock<IService7> mock = new();

            mock.SetupWithAny(nameof(IService7.ReturnValue));
        }

        [TestMethod]
        public void You_can_setup_with_any_on_a_method_with_a_return_value_using_AutoMocker()
        {
            string expected = "SomeValue";
            AutoMocker mocker = new();

            mocker.SetupWithAny<IService4, string>(nameof(IService4.MainMethodName))
                .Returns(expected);

            string result = mocker.Get<IService4>().MainMethodName("Something");

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void You_can_setup_with_any_on_a_method_with_a_return_value_using_AutoMocker_with_cached_mock()
        {
            string expected = "SomeValue";
            AutoMocker mocker = new();
            Mock<IService4> mock = new();
            mocker.Use(mock);

            mocker.SetupWithAny<IService4, string>(nameof(IService4.MainMethodName))
                .Returns(expected);

            string result = mock.Object.MainMethodName("Something");

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void You_can_setup_with_any_on_a_void_method_using_AutoMocker()
        {
            AutoMocker mocker = new();

            mocker.SetupWithAny<IService6>(nameof(IService6.Void))
                .Verifiable();

            mocker.Get<IService6>().Void(42, "SomeValue");

            mocker.VerifyAll();
        }

        [TestMethod]
        public void You_can_setup_with_any_on_a_void_method_using_AutoMocker_with_cached_mock()
        {
            AutoMocker mocker = new();
            Mock<IService6> mock = new();
            mocker.Use(mock);

            mocker.SetupWithAny<IService6>(nameof(IService6.Void))
                .Verifiable();

            mock.Object.Void(42, "SomeValue");

            mock.VerifyAll();
        }
    }
}
