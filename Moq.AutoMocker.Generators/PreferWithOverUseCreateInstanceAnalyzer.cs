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

        if (!TryGetAutoMockerReceiver(context.SemanticModel, useInvocation, "Use", context.CancellationToken, out var useReceiver))
        {
            return;
        }

        foreach (var argument in useInvocation.ArgumentList.Arguments)
        {
            if (Unwrap(argument.Expression) is not InvocationExpressionSyntax createInvocation)
            {
                continue;
            }

            if (!TryGetAutoMockerReceiver(context.SemanticModel, createInvocation, "CreateInstance", context.CancellationToken, out var createReceiver))
            {
                continue;
            }

            if (!HaveSameReceiver(useReceiver, createReceiver))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostics.PreferWithOverUseCreateInstance.Create(argument.GetLocation()));
            return;
        }
    }

    private static bool TryGetAutoMockerReceiver(
        SemanticModel semanticModel,
        InvocationExpressionSyntax invocation,
        string methodName,
        CancellationToken cancellationToken,
        out ExpressionSyntax receiver)
    {
        receiver = null!;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        if (memberAccess.Name.Identifier.ValueText != methodName)
        {
            return false;
        }

        var receiverType = semanticModel.GetTypeInfo(memberAccess.Expression, cancellationToken).Type;
        if (receiverType is null || receiverType.ToDisplayString() != AutoMockerTypeName)
        {
            return false;
        }

        receiver = Unwrap(memberAccess.Expression);
        return true;
    }

    private static bool HaveSameReceiver(
        ExpressionSyntax useReceiver,
        ExpressionSyntax createReceiver)
    {
        return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AreEquivalent(
            useReceiver.WithoutTrivia(),
            createReceiver.WithoutTrivia());
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
