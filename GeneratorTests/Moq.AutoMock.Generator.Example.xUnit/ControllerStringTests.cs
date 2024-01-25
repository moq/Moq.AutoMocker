namespace Moq.AutoMock.Generator.Example.xUnit;

[ConstructorTests(typeof(ControllerString), Behavior = TestGenerationBehavior.IgnoreNullableParameters)]
public partial class ControllerStringTests
{
    partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName)
    {
        mocker.Use<string>("");
        mocker.Use<int?>(42);
        mocker.Use<int>(24);
    }
}
