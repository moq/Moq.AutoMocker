using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeCombiningTypes
    {
        [TestMethod]
        public void It_uses_the_same_mock_for_all_instances()
        {
            var mocker = new AutoMocker();
            mocker.Combine(typeof(IService1), typeof(IService2),
                typeof(IService3));

            Assert.AreSame(mocker.Get<IService2>(), mocker.Get<IService1>());
            Assert.AreSame(mocker.Get<IService3>(), mocker.Get<IService2>());
        }

        //[TestMethod]
        //public void Convenience_methods_work()
        //{
        //    var mocker = new AutoMocker();
        //    mocker.Combine<IService1, IService2, IService3>();
        //
        //    Assert.AreSame(mocker.Get<IService2>(), mocker.Get<IService1>());
        //    Assert.AreSame(mocker.Get<IService3>(), mocker.Get<IService2>());
        //}

        [TestMethod]
        public void Combine_method_should_maintain_setups()
        {
            var mocker2 = new AutoMocker(MockBehavior.Loose);
            mocker2.GetMock<IDerivedInterface>().Setup(x => x.Foo()).Returns(() => "42");
            mocker2.Combine(typeof(IDerivedInterface), typeof(IBaseInterface));

            Assert.Equals("42", mocker2.Get<IBaseInterface>().Foo());
            Assert.Equals("42", mocker2.Get<IDerivedInterface>().Foo());
        }

        interface IBaseInterface
        {
            string Foo();
        }
        interface IDerivedInterface : IBaseInterface
        {
            string Bar();
        }
    }
}
