using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using Moq.AutoMock.Tests.Util;
using System;
using System.Linq;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeVerify
    {
        [TestMethod]
        public void It_throws_if_cache_is_not_registered()
        {
            AutoMocker mocker = new();
            mocker.Resolvers.Remove(mocker.Resolvers.OfType<CacheResolver>().Single());

            Assert.ThrowsException<InvalidOperationException>(() => mocker.Verify());
        }

        [TestMethod]
        public void It_throws_if_expression_is_null()
        {
            AutoMocker mocker = new();

            Assert.ThrowsException<ArgumentNullException>(() => mocker.Verify<object, object>(null!));
        }

        [TestMethod]
        public void It_verifies_mock_with_expression()
        {
            AutoMocker mocker = new();
            var mock = mocker.GetMock<IService7>();
            mock.Object.ReturnValue("foo");

            mocker.Verify<IService7, object>(x => x.ReturnValue("foo"));
        }

        [TestMethod]
        public void It_throws_if_expression_with_times_is_null()
        {
            AutoMocker mocker = new();

            Assert.ThrowsException<ArgumentNullException>(() => mocker.Verify<IService7, object>(null!, Times.Never()));
        }

        [TestMethod]
        public void It_verifies_mock_with_expression_and_times()
        {
            AutoMocker mocker = new();
            var mock = mocker.GetMock<IService7>();
            mock.Object.ReturnValue("foo");

            mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), Times.Once());
        }

        [TestMethod]
        public void It_throws_if_expression_with_times_func_has_null_expression()
        {
            AutoMocker mocker = new();

            Assert.ThrowsException<ArgumentNullException>(() => mocker.Verify<IService7, object>(null!, Times.Never));
        }

        [TestMethod]
        public void It_throws_if_expression_with_times_func_has_null_times_func()
        {
            AutoMocker mocker = new();

            Assert.ThrowsException<ArgumentNullException>(() => mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), (Func<Times>)null!));
        }

        [TestMethod]
        public void It_verifies_mock_with_expression_and_times_func()
        {
            AutoMocker mocker = new();
            var mock = mocker.GetMock<IService7>();
            mock.Object.ReturnValue("foo");

            mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), Times.Once);
        }

        [TestMethod]
        public void It_throws_if_expression_with_fail_message_has_null_expression()
        {
            AutoMocker mocker = new();

            Assert.ThrowsException<ArgumentNullException>(() => mocker.Verify<IService7, object>(null!, "fail"));
        }

        [TestMethod]
        public void It_verifies_mock_with_expression_and_fail_message()
        {
            AutoMocker mocker = new();
            var mock = mocker.GetMock<IService7>();
            mock.Object.ReturnValue("foo");

            mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), "fail");
        }

        [TestMethod]
        public void It_throws_if_expression_with_times_and_fail_message_has_null_expression()
        {
            AutoMocker mocker = new();

            Assert.ThrowsException<ArgumentNullException>(() => mocker.Verify<IService7, object>(null!, Times.Never(), "fail"));
        }

        [TestMethod]
        public void It_throws_if_expression_with_times_and_fail_message_has_null_fail_message()
        {
            AutoMocker mocker = new();

            Assert.ThrowsException<ArgumentNullException>(() => mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), Times.Never(), null!));
        }

        [TestMethod]
        public void It_verifies_mock_with_expression_and_times_and_fail_message()
        {
            AutoMocker mocker = new();
            var mock = mocker.GetMock<IService7>();
            mock.Object.ReturnValue("foo");

            mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), Times.Once(), "fail");
        }
    }
}
