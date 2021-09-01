using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests.Resolvers
{
    [TestClass]
    public class MockResolutionContextTests
    {
        [TestMethod]
        public void Constructor_null_checks_arguments()
            => ConstructorTest
            .BuildArgumentNullExceptionsTest<MockResolutionContext>()
            .Use(new ObjectGraphContext(false))
            .Run();
    }
}
