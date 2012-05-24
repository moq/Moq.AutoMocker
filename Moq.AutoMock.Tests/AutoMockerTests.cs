using Should;
using Xunit;

namespace Moq.AutoMock.Tests
{
    public class AutoMockerTests
    {
        #region Types used for testing

        private class Empty
        {
        }

        #endregion

        public class DescribeGetInstance
        {
            [Fact]
            public void It_creates_object_with_no_constructor()
            {
                var mocker = new AutoMocker();
                var instance = mocker.GetInstance<Empty>();
                instance.ShouldNotBeNull();
            }
        }
    }
}
