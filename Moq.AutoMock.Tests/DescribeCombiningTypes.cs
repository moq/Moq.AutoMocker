using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Tests
{
    [TestClass]
        public class DescribeCombiningTypes
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void It_uses_the_same_mock_for_all_instances()
            {
                mocker.Combine(typeof(IService1), typeof(IService2), 
                    typeof(IService3));

                Assert.AreSame(mocker.Get<IService2>(), mocker.Get<IService1>());
                Assert.AreSame(mocker.Get<IService3>(), mocker.Get<IService2>());
            }

            [TestMethod]
            public void Convenience_methods_work()
            {
                mocker.Combine<IService1, IService2, IService3>();

                Assert.AreSame(mocker.Get<IService2>(), mocker.Get<IService1>());
                Assert.AreSame(mocker.Get<IService3>(), mocker.Get<IService2>());
            }
        }
}
