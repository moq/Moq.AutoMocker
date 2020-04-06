using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class ConstructorSelectorTests
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;

        [TestMethod]
        public void It_chooses_the_ctor_with_arguments()
        {
            var ctor = typeof(WithDefaultAndSingleParameter).SelectCtor(Array.Empty<Type>(), DefaultBindingFlags);
            Assert.AreEqual(1, ctor.GetParameters().Length);
        }

        [TestMethod]
        public void It_chooses_the_ctor_with_the_most_arguments()
        {
            var ctor = typeof(With3Parameters).SelectCtor(Array.Empty<Type>(), DefaultBindingFlags);
            Assert.AreEqual(2, ctor.GetParameters().Length);
        }

        [TestMethod]
        public void It_chooses_the_ctor_with_the_most_arguments_when_arguments_are_arrays()
        {
            var ctor = typeof(WithArrayParameter).SelectCtor(Array.Empty<Type>(), DefaultBindingFlags);
            Assert.AreEqual(1, ctor.GetParameters().Length);
        }

        [TestMethod]
        public void It_wont_select_if_an_argument_is_sealed_and_not_array()
        {
            var ctor = typeof(WithSealedParameter).SelectCtor(Array.Empty<Type>(), DefaultBindingFlags);
            Assert.AreEqual(0, ctor.GetParameters().Length);
        }

        [TestMethod]
        public void It_will_select_if_an_argument_is_sealed_and_supplied()
        {
            var ctor = typeof(WithSealedParameter).SelectCtor(new Type[] { typeof(string) }, DefaultBindingFlags);
            Assert.AreEqual(1, ctor.GetParameters().Length);
        }

        [TestMethod]
        public void It_will_select_a_private_ctor_when_specified()
        {
            const BindingFlags privateBindingFlags = DefaultBindingFlags | BindingFlags.NonPublic;
            var ctor = typeof(WithPrivateConstructor).SelectCtor(Array.Empty<Type>(), privateBindingFlags);
            Assert.AreEqual(2, ctor.GetParameters().Length);
        }
    }
}
