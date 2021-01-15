using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using System;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class EnumerableResolverTests
    {
        [TestMethod]
        public void ChecksForNullContext()
        {
            EnumerableResolver resolver = new();
            Assert.ThrowsException<ArgumentNullException>(() => resolver.Resolve(null!));
        }
    }
}
