using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Moq.AutoMocker.Generators;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferWithOverUseCreateInstanceAnalyzer : DiagnosticAnalyzer
{
    private const string AutoMockerTypeName = "Moq.AutoMock.AutoMocker";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [Diagnostics.PreferWithOverUseCreateInstance.Descriptor];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, Microsoft.CodeAnalysis.CSharp.SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax useInvocation)
        {
            return;
        }

        if (!IsAutoMockerMethod(context.SemanticModel, useInvocation, "Use", out _))
        {
            return;
        }

        foreach (var argument in useInvocation.ArgumentList.Arguments)
        {
            if (Unwrap(argument.Expression) is not InvocationExpressionSyntax createInvocation)
            {
                continue;
            }

            if (!IsAutoMockerMethod(context.SemanticModel, createInvocation, "CreateInstance", out _))
            {
                continue;
            }

            if (!HaveSameReceiver(context.SemanticModel, useInvocation, createInvocation, context.CancellationToken))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostics.PreferWithOverUseCreateInstance.Create(argument.GetLocation()));
            return;
        }
    }

    private static bool IsAutoMockerMethod(
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation,
        string methodName,
        out IMethodSymbol? methodSymbol)
    {
        methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (methodSymbol is null)
        {
            return false;
        }

        return methodSymbol.Name == methodName &&
               methodSymbol.ContainingType.ToDisplayString() == AutoMockerTypeName;
    }

    private static bool HaveSameReceiver(
        SemanticModel semanticModel,
        InvocationExpressionSyntax useInvocation,
        InvocationExpressionSyntax createInvocation,
        CancellationToken cancellationToken)
    {
        var useReceiver = GetReceiverSymbol(semanticModel, useInvocation, cancellationToken);
        var createReceiver = GetReceiverSymbol(semanticModel, createInvocation, cancellationToken);
        return SymbolEqualityComparer.Default.Equals(useReceiver, createReceiver);
    }

    private static ISymbol? GetReceiverSymbol(SemanticModel semanticModel, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return null;
        }

        return semanticModel.GetSymbolInfo(memberAccess.Expression, cancellationToken).Symbol;
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }

        return expression;
    }
}
