using Should;
using Xunit;

namespace Moq.AutoMock.Tests
{
    public class ConstructorSelectorTests
    {
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
        #endregion

        [Fact]
        public void It_chooses_the_ctor_with_arguments()
        {
            var ctor = selector.SelectFor(typeof (WithDefaultAndSingleParameter));
            ctor.GetParameters().Length.ShouldEqual(1);
        }

        [Fact]
        public void It_chooses_the_ctor_with_the_most_arguments()
        {
            var ctor = selector.SelectFor(typeof (With3Parameters));
            ctor.GetParameters().Length.ShouldEqual(2);
        }

        [Fact]
        public void It_wont_select_if_an_argument_is_sealed()
        {
            var ctor = selector.SelectFor(typeof (WithSealedParameter));
            ctor.GetParameters().Length.ShouldEqual(0);
        }
    }
}
