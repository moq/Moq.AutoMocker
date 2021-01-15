using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using System;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class MockResolverTests
    {
        [TestMethod]
        public void ChecksForNullContext()
        {
            MockResolver resolver = new(MockBehavior.Default, DefaultValue.Empty, false);
            Assert.ThrowsException<ArgumentNullException>(() => resolver.Resolve(null!));
        }
    }
}
