namespace Moq.AutoMock.Generator.Example;

public class ControllerString
{
    public ControllerString(
        string name, 
        string? nullableName, 
        string? testName = null, 
        string foo = null!, 
        int? i = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        NullableName = nullableName;
        TestName = testName;
    }

    public string Name { get; } = "";
    public string? NullableName { get; }
    public string? TestName { get; }
}
