using System;
using System.Reflection;
using Xunit;

namespace Moq.AutoMock.Tests
{
    public class ConstructorSelectorTests
    {
        private const BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private readonly ConstructorSelector selector = new ConstructorSelector();
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

        [Fact]
        public void It_chooses_the_ctor_with_arguments()
        {
            var ctor = selector.SelectFor(typeof(WithDefaultAndSingleParameter), new Type[0], DefaultBindingFlags);
            Assert.Single(ctor.GetParameters());
        }

        [Fact]
        public void It_chooses_the_ctor_with_the_most_arguments()
        {
            var ctor = selector.SelectFor(typeof(With3Parameters), new Type[0], DefaultBindingFlags);
            Assert.Equal(2, ctor.GetParameters().Length);
        }

        [Fact]
        public void It_chooses_the_ctor_with_the_most_arguments_when_arguments_are_arrays()
        {
            var ctor = selector.SelectFor(typeof(WithArrayParameter), new Type[0], DefaultBindingFlags);
            Assert.Single(ctor.GetParameters());
        }

        [Fact]
        public void It_wont_select_if_an_argument_is_sealed_and_not_array()
        {
            var ctor = selector.SelectFor(typeof(WithSealedParameter), new Type[0], DefaultBindingFlags);
            Assert.Empty(ctor.GetParameters());
        }

        [Fact]
        public void It_will_select_if_an_argument_is_sealed_and_supplied()
        {
            var ctor = selector.SelectFor(typeof(WithSealedParameter), new Type[] { typeof(string) }, DefaultBindingFlags);
            Assert.Single(ctor.GetParameters());
        }

        [Fact]
        public void It_will_select_a_private_ctor_when_specified()
        {
            const BindingFlags privateBindingFlags = DefaultBindingFlags | BindingFlags.NonPublic;
            var ctor = selector.SelectFor(typeof(WithPrivateConstructor), new Type[0], privateBindingFlags);
            Assert.Equal(2, ctor.GetParameters().Length);
        }
    }
}
