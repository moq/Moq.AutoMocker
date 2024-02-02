using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Generator.Example.MSTest;

[TestClass]
[ConstructorTests(typeof(ControllerWithSomeNullableParameters), Behavior = TestGenerationBehavior.IgnoreNullableParameters)]
public partial class ControllerNullableParametersTests
{
    partial void ControllerWithSomeNullableParametersConstructor_WithNullstring_ThrowsArgumentNullExceptionSetup(AutoMocker mocker)
    {
        mocker.Use<string>("");
        mocker.Use<int?>(42);
        mocker.Use<int>(24);
    }
}
