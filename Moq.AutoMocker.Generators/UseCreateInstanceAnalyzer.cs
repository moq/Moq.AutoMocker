using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Moq.AutoMocker.Generators;

/// <summary>
/// Reports statements of the form <c>mocker.Use(mocker.CreateInstance&lt;T&gt;())</c>, suggesting
/// <c>mocker.With&lt;T&gt;()</c> instead.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCreateInstanceAnalyzer : DiagnosticAnalyzer
{
    private const string AutoMockerTypeName = "Moq.AutoMock.AutoMocker";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Diagnostics.PreferWith.Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            INamedTypeSymbol? autoMocker = compilationContext.Compilation.GetTypeByMetadataName(AutoMockerTypeName);
            if (autoMocker is null) return;

            compilationContext.RegisterOperationAction(
                operationContext => AnalyzeInvocation(operationContext, autoMocker),
                OperationKind.Invocation);
        });
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol autoMocker)
    {
        var use = (IInvocationOperation)context.Operation;
        IMethodSymbol useMethod = use.TargetMethod;
        // With returns the instance rather than the AutoMocker, so only suggest it when the result is unused.
        if (useMethod.Name != "Use"
            || !SymbolEqualityComparer.Default.Equals(useMethod.ContainingType, autoMocker)
            || use.Parent is not IExpressionStatementOperation
            || use.Instance is not { IsImplicit: false } useInstance)
        {
            return;
        }

        IArgumentOperation? valueArgument = use.Arguments.Length switch
        {
            1 when useMethod.IsGenericMethod => use.Arguments[0],
            2 when !useMethod.IsGenericMethod => use.Arguments[1],
            _ => null,
        };
        if (valueArgument is null) return;

        IOperation value = valueArgument.Value;
        while (value is IConversionOperation conversion)
        {
            value = conversion.Operand;
        }

        if (value is not IInvocationOperation create
            || create.TargetMethod.Name != "CreateInstance"
            || !SymbolEqualityComparer.Default.Equals(create.TargetMethod.ContainingType, autoMocker)
            || create.Instance is not { IsImplicit: false } createInstance
            || create.Arguments.Length != 1 - (useMethod.IsGenericMethod ? 1 : 0)
            || create.TargetMethod.IsGenericMethod != useMethod.IsGenericMethod
            || !SyntaxFactory.AreEquivalent(useInstance.Syntax, createInstance.Syntax))
        {
            return;
        }

        if (useMethod.IsGenericMethod
            && !IsImplicitReferenceOrIdentity(context.Compilation, create.TargetMethod.TypeArguments[0], useMethod.TypeArguments[0]))
        {
            return;
        }

        Location location = use.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess }
            ? memberAccess.Name.GetLocation()
            : use.Syntax.GetLocation();
        context.ReportDiagnostic(Diagnostics.PreferWith.Create(location));
    }

    private static bool IsImplicitReferenceOrIdentity(Compilation compilation, ITypeSymbol from, ITypeSymbol to)
    {
        Conversion conversion = ((CSharpCompilation)compilation).ClassifyConversion(from, to);
        return conversion.IsImplicit && (conversion.IsIdentity || conversion.IsReference);
    }
}
