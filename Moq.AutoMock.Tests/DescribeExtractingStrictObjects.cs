using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Tests
{
    [TestClass]
        public class DescribeExtractingStrictObjects
        {
            private readonly AutoMocker mocker = new AutoMocker(MockBehavior.Strict);

            [TestMethod]
            public void It_creates_a_mock_as_strict_if_the_object_is_missing()
            {
                var mock = mocker.GetMock<IService1>();
                Assert.AreEqual(MockBehavior.Strict,mock.Behavior);
            }
        }
}
