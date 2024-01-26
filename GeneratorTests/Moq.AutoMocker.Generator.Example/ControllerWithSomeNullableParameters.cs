namespace Moq.AutoMock.Generator.Example;

public class ControllerWithSomeNullableParameters
{
    public ControllerWithSomeNullableParameters(
        string name, // Test generated
        int years, // No test generated
        string? nullableName, // No test generated
        string? testName = null, // No test generated
        string foo = null!, // No test generated
        int? age = null) //No test generated
    {
        ArgumentNullException.ThrowIfNull(name);
    }
}
