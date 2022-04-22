namespace Moq.AutoMock.Generator.Example.NUnit;

[ConstructorTests(TargetType = typeof(Controller))]
public partial class ControllerTests
{
    partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName)
    {
        mocker.Use<string>("");
    }
}
