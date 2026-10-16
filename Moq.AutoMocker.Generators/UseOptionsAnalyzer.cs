using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Moq.AutoMocker.Generators;

/// <summary>
/// Reports calls to AutoMocker.Use that pass an IOptions&lt;T&gt; instance, suggesting the
/// source generated WithOptions extension method instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseOptionsAnalyzer : DiagnosticAnalyzer
{
    private const string AutoMockerTypeName = "Moq.AutoMock.AutoMocker";
    private const string OptionsExtensionsTypeName = "Moq.AutoMock.AutoMockerOptionsExtensions";
    private const string OptionsTypeName = "Microsoft.Extensions.Options.IOptions`1";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Diagnostics.PreferWithOptions.Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            Compilation compilation = compilationContext.Compilation;
            INamedTypeSymbol? autoMocker = compilation.GetTypeByMetadataName(AutoMockerTypeName);
            INamedTypeSymbol? optionsType = compilation.GetTypeByMetadataName(OptionsTypeName);
            // Only suggest WithOptions when the generated extension is actually available.
            if (autoMocker is null || optionsType is null
                || compilation.GetTypeByMetadataName(OptionsExtensionsTypeName) is null)
            {
                return;
            }

            compilationContext.RegisterOperationAction(
                operationContext => AnalyzeInvocation(operationContext, autoMocker, optionsType),
                OperationKind.Invocation);
        });
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol autoMocker, INamedTypeSymbol optionsType)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        if (method.Name != "Use"
            || !SymbolEqualityComparer.Default.Equals(method.ContainingType, autoMocker))
        {
            return;
        }

        ITypeSymbol? optionValueType = null;
        foreach (ITypeSymbol typeArgument in method.TypeArguments)
        {
            optionValueType = GetOptionsValueType(typeArgument, optionsType);
            if (optionValueType is not null) break;
        }

        if (optionValueType is null)
        {
            foreach (IArgumentOperation argument in invocation.Arguments)
            {
                IOperation value = argument.Value;
                while (value is IConversionOperation conversion)
                {
                    value = conversion.Operand;
                }
                if (value.Type is { } type && GetOptionsValueType(type, optionsType) is { } found)
                {
                    optionValueType = found;
                    break;
                }
            }
        }

        if (optionValueType is null) return;

        Location location = invocation.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess }
            ? memberAccess.Name.GetLocation()
            : invocation.Syntax.GetLocation();
        context.ReportDiagnostic(Diagnostics.PreferWithOptions.Create(location, optionValueType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
    }

    private static ITypeSymbol? GetOptionsValueType(ITypeSymbol type, INamedTypeSymbol optionsType)
    {
        if (type is INamedTypeSymbol named && SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, optionsType))
        {
            return named.TypeArguments[0];
        }

        foreach (INamedTypeSymbol @interface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(@interface.OriginalDefinition, optionsType))
            {
                return @interface.TypeArguments[0];
            }
        }
        return null;
    }
}
