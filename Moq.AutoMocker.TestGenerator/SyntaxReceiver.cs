using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Moq.AutoMocker.TestGenerator;

public class SyntaxReceiver : ISyntaxContextReceiver
{
    public List<GeneratorTargetClass> TestClasses { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            //Microsoft.CodeAnalysis.CSharp.SyntaxKind.
            if (classDeclaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            {
                string testClassName = classDeclaration.Identifier.Text;
                string namespaceDeclaration = string.Join(".", GetNamespaces(classDeclaration));

                TestClasses.Add(new GeneratorTargetClass
                {
                    Namespace = namespaceDeclaration,
                    TestClassName = testClassName,
                    Sut = new SutClass
                    {
                        Name = "Controller",
                        FullName = "Moq.AutoMock.Generator.Example.Controller",
                        NullConstructorParameterTests =
                        {
                            new NullConstructorParameterTest()
                            {
                                NullTypeName = "IService",
                                NullTypeFullName = "Moq.AutoMock.Generator.Example.IService",
                                ParameterName = "service"
                            }
                        }
                    }
                });
            }
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
