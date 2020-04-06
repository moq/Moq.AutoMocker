using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using System;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class ResolvesLazy
    {
        [TestMethod] 
        public void ResolvesLazyObjectFromContainer() => Resolves(new object());
        
        [TestMethod] 
        public void ResolvesLazyNumberFromContainer() => Resolves(42L);
        
        [TestMethod] 
        public void ResolvesLazyStringFromContainer() => Resolves(nameof(ResolvesLazy));

        private static void Resolves<T>(T expected)
        {
            var mocker = new AutoMocker { Resolvers = { new LazyResolver() } };
            mocker.Use(typeof(T), expected!);

            var lazy = mocker.Get<Lazy<T>>();
            Assert.IsNotNull(lazy);
            Assert.AreEqual(expected, lazy!.Value);
        }
    }
}
