using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class ResolvesEnumerables
    {
        [TestMethod]
        public void ResolvesObjectEnumerableFromContainer() => Resolves(new object());

        [TestMethod]
        public void ResolvesNumberEnumerableFromContainer() => Resolves(42L);

        [TestMethod]
        public void ResolvesStringEnumerableFromContainer() => Resolves(nameof(ResolvesEnumerables));

        private static void Resolves<T>(T expected)
        {
            var mocker = new AutoMocker { Resolvers = { new EnumerableResolver() } };
            mocker.Use(expected!);

            var enumerable = mocker.Get<IEnumerable<T>>();
            Assert.IsNotNull(enumerable);
            CollectionAssert.AreEquivalent(enumerable.ToArray(), new[] { expected });
        }
    }
}