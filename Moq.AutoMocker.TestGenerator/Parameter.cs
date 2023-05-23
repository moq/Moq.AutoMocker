using Microsoft.CodeAnalysis;

namespace Moq.AutoMocker.TestGenerator;

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
