using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;
using VerifyMSTest;

namespace Moq.AutoMock.Tests
{

    [TestClass]
    public class DescribeVerifyAll : VerifyBase
    {
        [TestMethod]
        public Task It_calls_VerifyAll_on_all_objects_that_are_mocks()
        {
            var mocker = new AutoMocker();
            mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
            var _ = mocker.CreateInstance<WithService>();
            return Throws(() => mocker.VerifyAll());
        }

        [TestMethod]
        public void It_doesnt_call_VerifyAll_if_the_object_isnt_a_mock()
        {
            var mocker = new AutoMocker();
            mocker.Use<IService2>(new Service2());
            mocker.CreateInstance<WithService>();
            mocker.VerifyAll();
        }
    }
}
