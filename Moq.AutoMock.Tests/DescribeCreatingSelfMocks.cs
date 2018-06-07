using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeCreatingSelfMocks
    {
        [TestMethod]
        public void Self_mocks_are_useful_for_testing_most_of_class()
        {
            var mocker = new AutoMocker();
            var selfMock = mocker.CreateSelfMock<InsecureAboutSelf>();
            selfMock.TellJoke();
            Assert.IsFalse(selfMock.SelfDepricated);
        }

        [TestMethod]
        public void It_can_self_mock_objects_with_constructor_arguments()
        {
            var mocker = new AutoMocker();
            var selfMock = mocker.CreateSelfMock<WithService>();
            Assert.IsNotNull(selfMock.Service);
            Assert.IsNotNull(Mock.Get(selfMock.Service));
        }
    }
}
