using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Generator.Example.MSTest;

[TestClass]
[ConstructorTests(TargetType = typeof(Controller))]
public partial class ControllerTests
{
    partial void ControllerConstructor_WithNullIService3_ThrowsArgumentNullExceptionSetup(AutoMocker mocker)
    {
        mocker.Use<string>("");
    }
}

