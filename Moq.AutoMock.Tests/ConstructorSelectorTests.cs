using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class ConstructorSelectorTests
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;

        #region Types used for testing
        class WithDefaultAndSingleParameter
        {
            public WithDefaultAndSingleParameter() { }
            public WithDefaultAndSingleParameter(IService1 service1) { }
        }

        class With3Parameters
        {
            public With3Parameters() { }
            public With3Parameters(IService1 service1) { }
            public With3Parameters(IService1 service1, IService2 service2) { }
        }

        class WithSealedParameter
        {
            public WithSealedParameter() {}
            public WithSealedParameter(string @sealed) {}
        }

        class WithArrayParameter
        {
            public WithArrayParameter() { }
            public WithArrayParameter(string[] array) { }
            public WithArrayParameter(string[] array, string @sealed) { }
        }

        class WithPrivateConstructor
        {
            public WithPrivateConstructor(IService1 service1) { }
            private WithPrivateConstructor(IService1 service1, IService2 service2) { }
        }
        #endregion

        [TestMethod]
        public void It_chooses_the_ctor_with_arguments()
        {
            var ctor = typeof(WithDefaultAndSingleParameter).SelectCtor(new Type[0], DefaultBindingFlags);
            Assert.AreEqual(1, ctor.GetParameters().Length);
        }

        [TestMethod]
        public void It_chooses_the_ctor_with_the_most_arguments()
        {
            var ctor = typeof(With3Parameters).SelectCtor(new Type[0], DefaultBindingFlags);
            Assert.AreEqual(2, ctor.GetParameters().Length);
        }

        [TestMethod]
        public void It_chooses_the_ctor_with_the_most_arguments_when_arguments_are_arrays()
        {
            var ctor = typeof(WithArrayParameter).SelectCtor(new Type[0], DefaultBindingFlags);
            Assert.AreEqual(1, ctor.GetParameters().Length);
        }

        [TestMethod]
        public void It_wont_select_if_an_argument_is_sealed_and_not_array()
        {
            var ctor = typeof(WithSealedParameter).SelectCtor(new Type[0], DefaultBindingFlags);
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
            var ctor = typeof(WithPrivateConstructor).SelectCtor(new Type[0], privateBindingFlags);
            Assert.AreEqual(2, ctor.GetParameters().Length);
        }
    }
}
