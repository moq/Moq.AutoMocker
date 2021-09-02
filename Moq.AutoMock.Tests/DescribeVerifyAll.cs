using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using Moq.AutoMock.Tests.Util;
using System;
using System.Linq;

namespace Moq.AutoMock.Tests
{

    [TestClass]
    public class DescribeVerifyAll
    {
        [TestMethod]
        public void It_calls_VerifyAll_on_all_objects_that_are_mocks()
        {
            var mocker = new AutoMocker();
            mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
            var _ = mocker.CreateInstance<WithService>();
            var ex = Assert.ThrowsException<MockException>(() => mocker.VerifyAll());
            Assert.IsTrue(ex.IsVerificationError);
        }

        [TestMethod]
        public void It_doesnt_call_VerifyAll_if_the_object_isnt_a_mock()
        {
            var mocker = new AutoMocker();
            mocker.Use<IService2>(new Service2());
            mocker.CreateInstance<WithService>();
            mocker.VerifyAll();
        }

        [TestMethod]
        public void It_throws_if_cache_is_not_registered()
        {
            AutoMocker mocker = new();
            mocker.Resolvers.Remove(mocker.Resolvers.OfType<CacheResolver>().Single());

            Assert.ThrowsException<InvalidOperationException>(() => mocker.VerifyAll());
        }
    }
}
