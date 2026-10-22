using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Moq.AutoMocker.Generators;

/// <summary>
/// Reports unnecessary registrations of unconfigured or already retained mocks.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RedundantUseAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Diagnostics.RedundantUse.Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            INamedTypeSymbol? autoMocker = compilationContext.Compilation.GetTypeByMetadataName("Moq.AutoMock.AutoMocker");
            INamedTypeSymbol? mock = compilationContext.Compilation.GetTypeByMetadataName("Moq.Mock`1");
            INamedTypeSymbol? mockBase = compilationContext.Compilation.GetTypeByMetadataName("Moq.Mock");
            if (autoMocker is null || mock is null || mockBase is null) return;

            compilationContext.RegisterOperationAction(
                operationContext => AnalyzeInvocation(operationContext, autoMocker, mock, mockBase),
                OperationKind.Invocation);
        });
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context, INamedTypeSymbol autoMocker,
        INamedTypeSymbol mock, INamedTypeSymbol mockBase)
    {
        var use = (IInvocationOperation)context.Operation;
        if (use.TargetMethod.Name != "Use"
            || !SymbolEqualityComparer.Default.Equals(use.TargetMethod.ContainingType, autoMocker)
            || use.Instance is null)
        {
            return;
        }

        IArgumentOperation? argument = use.Arguments.FirstOrDefault(a => a.Parameter?.Name is "service" or "mockedService");
        if (argument is null) return;
        ITypeSymbol? registrationType = GetServiceType(use);
        if (registrationType is null || registrationType.TypeKind == TypeKind.Class) return;

        if (use.SemanticModel is not { } model) return;
        IOperation value = UnwrapMockObject(argument.Value, mock, mockBase);
        ILocalSymbol? local = null;
        LocalDeclarationStatementSyntax? declaration = null;
        if (value is ILocalReferenceOperation reference)
        {
            local = reference.Local;
            if (local.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax(context.CancellationToken)
                    is not VariableDeclaratorSyntax { Initializer.Value: { } initializer } declarator
                || declarator.Parent?.Parent is not LocalDeclarationStatementSyntax localDeclaration
                || localDeclaration.Parent is not BlockSyntax block
                || use.Syntax.Ancestors().OfType<StatementSyntax>().FirstOrDefault() is not { } statement
                || statement.Parent != block
                || localDeclaration.SpanStart >= statement.SpanStart
                || model.AnalyzeDataFlow(localDeclaration) is not { Succeeded: true } flow
                || flow.WrittenOutside.Contains(local, SymbolEqualityComparer.Default))
            {
                return;
            }

            declaration = localDeclaration;
            if (model.GetOperation(initializer, context.CancellationToken) is not { } origin) return;
            value = UnwrapMockObject(origin, mock, mockBase);
        }

        bool retained = false;
        ITypeSymbol? serviceType = null;
        if (value is IInvocationOperation get
            && get.TargetMethod.Name == "GetMock"
            && SymbolEqualityComparer.Default.Equals(get.TargetMethod.ContainingType, autoMocker)
            && get.Instance is { } instance
            && SameReceiver(use.Instance, instance)
            && get.Arguments.All(a => Unwrap(a.Value) is ITypeOfOperation
                || a.Value.ConstantValue is { HasValue: true, Value: bool }))
        {
            serviceType = GetServiceType(get);
            retained = true;
            // An intervening use of the container could replace the registration or the container itself.
            if (declaration is not null && declaration.Parent!.DescendantNodes()
                .OfType<ExpressionSyntax>()
                .Where(n => n.SpanStart >= declaration.Span.End && n.Span.End <= use.Syntax.SpanStart)
                .Any(n => model.GetOperation(n, context.CancellationToken) is { } operation
                    && SameReceiver(operation, use.Instance)))
            {
                return;
            }
        }
        else if (value is IObjectCreationOperation { Arguments.Length: 0, Initializer: null, Type: INamedTypeSymbol type }
            && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, mock))
        {
            serviceType = type.TypeArguments[0];
        }
        else if (value is IInvocationOperation { TargetMethod.Name: "Of", Arguments.Length: 0 } of
            && SymbolEqualityComparer.Default.Equals(of.TargetMethod.ContainingType, mockBase))
        {
            serviceType = of.TargetMethod.TypeArguments[0];
        }

        if (!SymbolEqualityComparer.Default.Equals(serviceType, registrationType)) return;

        // A separately created mock may be configured, verified, or shared even after Use.
        if (!retained && local is not null && declaration!.Parent!.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Any(n => !argument.Syntax.Span.Contains(n.Span)
                && SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(n, context.CancellationToken).Symbol, local)))
        {
            return;
        }

        Location location = use.Syntax is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccess }
            ? memberAccess.Name.GetLocation()
            : use.Syntax.GetLocation();
        context.ReportDiagnostic(Diagnostics.RedundantUse.Create(location, retained));
    }

    private static ITypeSymbol? GetServiceType(IInvocationOperation invocation)
    {
        if (invocation.TargetMethod.IsGenericMethod) return invocation.TargetMethod.TypeArguments[0];
        IArgumentOperation? argument = invocation.Arguments.FirstOrDefault(a => a.Parameter?.Ordinal == 0);
        return argument is not null && Unwrap(argument.Value) is ITypeOfOperation type ? type.TypeOperand : null;
    }

    private static IOperation Unwrap(IOperation operation)
    {
        while (true)
        {
            switch (operation)
            {
                case IConversionOperation { OperatorMethod: null } conversion:
                    operation = conversion.Operand;
                    break;
                case IParenthesizedOperation parenthesized:
                    operation = parenthesized.Operand;
                    break;
                default:
                    return operation;
            }
        }
    }

    private static IOperation UnwrapMockObject(IOperation operation, INamedTypeSymbol mock, INamedTypeSymbol mockBase)
    {
        operation = Unwrap(operation);
        if (operation is IPropertyReferenceOperation { Property.Name: "Object", Instance: { } instance } property
            && (SymbolEqualityComparer.Default.Equals(property.Property.ContainingType.OriginalDefinition, mock)
                || SymbolEqualityComparer.Default.Equals(property.Property.ContainingType, mockBase)))
        {
            return Unwrap(instance);
        }
        return operation;
    }

    private static bool SameReceiver(IOperation left, IOperation right) => (Unwrap(left), Unwrap(right)) switch
    {
        (ILocalReferenceOperation a, ILocalReferenceOperation b) => SymbolEqualityComparer.Default.Equals(a.Local, b.Local),
        (IParameterReferenceOperation a, IParameterReferenceOperation b) => SymbolEqualityComparer.Default.Equals(a.Parameter, b.Parameter),
        (IInstanceReferenceOperation a, IInstanceReferenceOperation b) => a.ReferenceKind == b.ReferenceKind,
        (IFieldReferenceOperation { Instance: IInstanceReferenceOperation } a,
            IFieldReferenceOperation { Instance: IInstanceReferenceOperation } b) => SymbolEqualityComparer.Default.Equals(a.Field, b.Field),
        _ => false,
    };
}
