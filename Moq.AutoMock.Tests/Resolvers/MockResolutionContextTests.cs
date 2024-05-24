using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests.Resolvers;

[TestClass]
[ConstructorTests(typeof(MockResolutionContext))]
public partial class MockResolutionContextTests
{
    partial void AutoMockerTestSetup(AutoMocker mocker, string testName)
    {
        mocker.Use(new ObjectGraphContext(false));
    }
}
