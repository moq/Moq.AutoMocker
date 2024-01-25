namespace Moq.AutoMock.Generator.Example;

public class ControllerString
{
    public ControllerString(
        string name,
        int years,
        string? nullableName, 
        string? testName = null, 
        string foo = null!, 
        int? age = null)
    {
        ArgumentNullException.ThrowIfNull(name);
    }

}
