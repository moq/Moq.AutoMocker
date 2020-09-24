using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;
using VerifyMSTest;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeCombiningTypes : VerifyBase
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

        [TestMethod]
        public void Convenience_methods_work()
        {
            var mocker = new AutoMocker();
            mocker.Combine<IService1, IService2, IService3>();

            Assert.AreSame(mocker.Get<IService2>(), mocker.Get<IService1>());
            Assert.AreSame(mocker.Get<IService3>(), mocker.Get<IService2>());
        }
    }
}
