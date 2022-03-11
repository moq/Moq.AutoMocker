namespace Moq.AutoMocker.TestGenerator;

public class GeneratorTargetClass
{
    public string? Namespace { get; set; }
    public string? TestClassName { get; set; }

    public SutClass? Sut { get; set; }
}


public class SutClass
{
    public string? Name { get; set; }
    public string? FullName { get; set; }

    public List<NullConstructorParameterTest> NullConstructorParameterTests { get; } = new();
}

public class NullConstructorParameterTest
{
    public string? NullTypeName { get; set; }
    public string? NullTypeFullName { get; set; }
    public string? ParameterName { get; set; }
}
