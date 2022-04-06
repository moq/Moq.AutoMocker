using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Moq.AutoMocker.TestGenerator;

public class SyntaxReceiver : ISyntaxContextReceiver
{
    public List<GeneratorTargetClass> TestClasses { get; } = new();

    private TypeSyntax? GetTargetType(AttributeSyntax attributeSyntax)
    {
        if (attributeSyntax.ArgumentList?.Arguments.Count > 0 &&
            attributeSyntax.ArgumentList.Arguments
                .Select(x => x.Expression)
                .OfType<TypeOfExpressionSyntax>()
                .FirstOrDefault() is { } typeExpression)
        {
            return typeExpression.Type;
        }
        return null;
    }

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration &&
            classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)) &&
            context.SemanticModel.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol symbol &&
            classDeclaration.AttributeLists.SelectMany(x => x.Attributes)
                .Select(a =>
                {
                    if (context.SemanticModel.GetTypeInfo(a).Type?.Name == "ConstructorTestsAttribute" &&
                        GetTargetType(a) is { } targetType &&
                        context.SemanticModel.GetTypeInfo(targetType).Type is INamedTypeSymbol sutType)
                    {
                        return sutType;
                    }
                    return null;
                })
                .FirstOrDefault(a => a is not null) is { } sutType
            )
        {
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

            //TestClasses.Add(new GeneratorTargetClass
            //{
            //    Namespace = namespaceDeclaration,
            //    TestClassName = testClassName,
            //    Sut = new SutClass
            //    {
            //        Name = "Controller",
            //        FullName = "Moq.AutoMock.Generator.Example.Controller",
            //        NullConstructorParameterTests =
            //        {
            //            new NullConstructorParameterTest()
            //            {
            //                NullTypeName = "IService",
            //                NullTypeFullName = "Moq.AutoMock.Generator.Example.IService",
            //                ParameterName = "service"
            //            }
            //        }
            //    }
            //});
        }
    }

    private static IEnumerable<string> GetNamespaces(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var @namespace in classDeclaration
            .Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .Reverse())
        {
            yield return @namespace.Name.ToFullString();
        }
    }
}
