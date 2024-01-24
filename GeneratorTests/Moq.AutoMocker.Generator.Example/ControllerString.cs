namespace Moq.AutoMock.Generator.Example;

public class ControllerString
{
    public ControllerString(string name, string? testName = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        TestName = testName;
    }

    public string Name { get; } = "";
    public string? TestName { get; }
}
