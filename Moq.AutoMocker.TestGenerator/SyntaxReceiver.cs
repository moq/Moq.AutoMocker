using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Moq.AutoMocker.TestGenerator;

public class SyntaxReceiver : ISyntaxContextReceiver
{
    public List<GeneratorTargetClass> TestClasses { get; } = new();

    public List<Diagnostic> DiagnosticMessages { get; } = new();

    private TypeSyntax? GetTargetType(AttributeSyntax attributeSyntax)
    {
        return attributeSyntax.ArgumentList?.Arguments.Count > 0 &&
            attributeSyntax.ArgumentList.Arguments
                .Select(x => x.Expression)
                .OfType<TypeOfExpressionSyntax>()
                .FirstOrDefault() is { } typeExpression
            ? typeExpression.Type
            : null;
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration &&
            context.SemanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol symbol &&
            classDeclaration.AttributeLists.SelectMany(x => x.Attributes)
                .Select(a =>
                {
                    if (context.SemanticModel.GetTypeInfo(a).Type?.Name == AutoMock.ConstructorTestsAttribute)
                    {
                        if (GetTargetType(a) is { } targetType &&
                            context.SemanticModel.GetTypeInfo(targetType).Type is INamedTypeSymbol sutType)
                        {
                            return sutType;
                        }
                        Diagnostic diagnostic = Diagnostics.MustSpecifyTargetType.Create(a.GetLocation(),
                            symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                        DiagnosticMessages.Add(diagnostic);
                    }
                    return null;
                })
                .FirstOrDefault(a => a is not null) is { } sutType
            )
        {
            if (!classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            {
                Diagnostic diagnostic = Diagnostics.TestClassesMustBePartial.Create(classDeclaration.GetLocation(),
                    symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                DiagnosticMessages.Add(diagnostic);
                return;
            }
            string testClassName = symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            string namespaceDeclaration = symbol.ContainingNamespace.ToDisplayString();

            SutClass sut = new()
            {
                Name = sutType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                FullName = sutType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            };

            foreach (IMethodSymbol ctor in sutType.Constructors)
            {
                var parameters = ctor.Parameters.Select(x => new Parameter(x)).ToList();
                int nullIndex = 0;
                foreach (IParameterSymbol parameter in ctor.Parameters)
                {
                    sut.NullConstructorParameterTests.Add(new NullConstructorParameterTest()
                    {
                        Parameters = parameters,
                        NullParameterIndex = nullIndex,
                        NullTypeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                    });
                    nullIndex++;
                }
            }

            GeneratorTargetClass targetClass = new()
            {
                Namespace = namespaceDeclaration,
                TestClassName = testClassName,
                Sut = sut
            };

            TestClasses.Add(targetClass);
        }
    }
}
