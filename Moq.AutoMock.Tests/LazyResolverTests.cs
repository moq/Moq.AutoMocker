using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using System;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class LazyResolverTests
    {
        [TestMethod]
        public void ChecksForNullContext()
        {
            LazyResolver resolver = new();
            Assert.ThrowsException<ArgumentNullException>(() => resolver.Resolve(null!));
        }
    }
}
