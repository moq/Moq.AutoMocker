using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeUsingExplicitObjects
    {
        [TestMethod]
        public void You_can_Use_an_instance_as_an_argument_to_GetInstance()
        {
            var mocker = new AutoMocker();
            var empty = new Empty();
            mocker.Use(empty);
            var instance = mocker.CreateInstance<OneConstructor>();
            Assert.AreSame(empty, instance.Empty);
        }

        [TestMethod]
        public void You_can_use_Use_as_an_alias_for_MockOf()
        {
            var mocker = new AutoMocker();
            mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
            var instance = mocker.CreateInstance<WithService>();
            Assert.IsInstanceOfType(instance.Service, typeof(IService2));
            Assert.IsInstanceOfType(instance.Service.Other, typeof(IService1));
        }

        [TestMethod]
        public void Adding_an_instance_will_replace_existing_setups()
        {
            var mocker = new AutoMocker();
            mocker.Use<IService2>(x => x.Other!.ToString() == "kittens");
            var otherService = Mock.Of<IService2>();
            mocker.Use(otherService);
            Assert.AreSame(otherService, mocker.Get<IService2>());
        }
    }
}
