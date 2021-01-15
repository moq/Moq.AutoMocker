using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using System;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class FuncResolverTests
    {
        [TestMethod]
        public void ChecksForNullContext()
        {
            FuncResolver resolver = new();
            Assert.ThrowsException<ArgumentNullException>(() => resolver.Resolve(null!));
        }
    }
}
