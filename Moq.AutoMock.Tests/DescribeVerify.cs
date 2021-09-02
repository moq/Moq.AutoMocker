using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
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
            mocker.Resolvers.Remove(mocker.Resolvers.OfType<CacheResolver>().Single());

            Assert.ThrowsException<InvalidOperationException>(() => mocker.Verify());
        }
    }
}
