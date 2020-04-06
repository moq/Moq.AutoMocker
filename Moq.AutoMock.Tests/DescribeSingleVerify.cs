using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeSingleVerify
    {
        [TestMethod]
        public void You_can_verify_a_single_method_call_directly()
        {
            var mocker = new AutoMocker();
            var mock = new Mock<IService2>();
            mocker.Use(mock);
            var _ = mock.Object.Name;
            mocker.Verify<IService2>(x => x.Name!);
        }

        [TestMethod]
        public void You_can_verify_a_method_that_returns_a_value_type()
        {
            var mocker = new AutoMocker();
            mocker.Setup<IServiceWithPrimitives, long>(s => s.ReturnsALong()).Returns(100L);

            var mock = mocker.Get<IServiceWithPrimitives>();
            Assert.AreEqual(100L, mock!.ReturnsALong());

            mocker.Verify<IServiceWithPrimitives, long>(s => s.ReturnsALong(), Times.Once());
        }

        [TestMethod]
        public void You_can_verify_all_setups_marked_as_verifiable()
        {
            var mocker = new AutoMocker();
            mocker.Setup<IService1>(x => x.Void()).Verifiable();
            mocker.Setup<IService5, string>(x => x.Name).Returns("Test");

            mocker.Get<IService1>()!.Void();

            mocker.Verify();
        }

        [TestMethod]
        public void If_you_verify_a_method_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
        {
            var mocker = new AutoMocker();

            //a method without parameters
            var ex = Assert.ThrowsException<NotSupportedException>(() => mocker.Verify<IServiceWithPrimitives>(s => s.ReturnsALong(), Times.Once()));
            Assert.AreEqual("Use the Verify overload that allows specifying TReturn if the setup returns a value type", ex.Message);
        }
    }
}
