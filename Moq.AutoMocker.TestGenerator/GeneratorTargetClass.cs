using Microsoft.CodeAnalysis;

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
    public List<Parameter>? Parameters { get; set; }
    public int NullParameterIndex { get; set; }
    public string? NullTypeName { get; set; }
    public string? NullParameterName => Parameters?[NullParameterIndex].Name;
}

public class Parameter
{
    public Parameter(IParameterSymbol symbol)
    {
        Symbol = symbol;
    }
    private  IParameterSymbol Symbol { get; }

    public string Name => Symbol.Name;
    public string ParameterType => Symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}
