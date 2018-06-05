using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Tests
{
    [TestClass]
        public class DescribeCreatingSelfMocks
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void Self_mocks_are_useful_for_testing_most_of_class()
            {
                var selfMock = mocker.CreateSelfMock<InsecureAboutSelf>();
                selfMock.TellJoke();
                Assert.IsFalse(selfMock.SelfDepricated);
            }

            [TestMethod]
            public void It_can_self_mock_objects_with_constructor_arguments()
            {
                var selfMock = mocker.CreateSelfMock<WithService>();
                Assert.IsNotNull(selfMock.Service);
                Assert.IsNotNull(Mock.Get(selfMock.Service));
            }
        }
}
