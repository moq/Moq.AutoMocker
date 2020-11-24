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

        [TestMethod]
        public void ResolvesIntEnumerableEnumerableFromContainer()
        {
            var mocker = new AutoMocker();
            mocker.Use(42);

            var enumerable = mocker.Get<IEnumerable<IEnumerable<int>>>();
            
            Assert.IsNotNull(enumerable);

            var outerArray = enumerable.ToArray();
            Assert.AreEqual(1, outerArray.Length);

            var innerArray = outerArray[0].ToArray();
            CollectionAssert.AreEquivalent(innerArray, new[] { 42 });
        }

        private static void Resolves<T>(T expected)
        {
            var mocker = new AutoMocker();
            mocker.Use(expected!);

            var enumerable = mocker.Get<IEnumerable<T>>();
            Assert.IsNotNull(enumerable);
            CollectionAssert.AreEquivalent(enumerable.ToArray(), new[] { expected });
        }
    }
}
