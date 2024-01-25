namespace Moq.AutoMock.Generator.Example.xUnit;

[ConstructorTests(typeof(ControllerString), Behavior = TestGenerationBehavior.SkipNullableReferenceTypes)]
public partial class ControllerStringTests
{
    partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName)
    {
        mocker.Use<string>("");
    }
}

[ConstructorTests(typeof(Controller), Behavior = TestGenerationBehavior.SkipNullableReferenceTypes)]
public partial class ControllerTests
{

}
