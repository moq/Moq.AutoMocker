using Microsoft.CodeAnalysis;

namespace Moq.AutoMocker.Generators;

public sealed class GeneratorTargetClass
{
    public static GeneratorTargetClass Empty { get; } = new();

    public string? Namespace { get; set; }
    public string? TestClassName { get; set; }

    public SutClass? Sut { get; set; }

    public bool SkipNullableParameters { get; set; }
}


public sealed class SutClass
{
    public string? Name { get; set; }
    public string? FullName { get; set; }

    public List<NullConstructorParameterTest> NullConstructorParameterTests { get; } = [];
}

public sealed class NullConstructorParameterTest
{
    public List<Parameter>? Parameters { get; set; }
    public int NullParameterIndex { get; set; }
    public string? NullTypeName { get; set; }
    public string? NullParameterName => Parameters?[NullParameterIndex].Name;
}

public sealed class Parameter
{
    public Parameter(IParameterSymbol symbol)
    {
        Name = symbol.Name;
        ParameterType = symbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        IsValueType = symbol.Type.IsValueType;

        if (symbol.HasExplicitDefaultValue)
        {
            IsNullable = symbol.ExplicitDefaultValue is null;
        }
        else if (symbol.NullableAnnotation is NullableAnnotation.Annotated)
        {
            IsNullable = true;
        }
    }

    public bool IsValueType { get; }
    public bool IsNullable { get; }
    public string Name { get; }
    public string ParameterType { get; }
}
