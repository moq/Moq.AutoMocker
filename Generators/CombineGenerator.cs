using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Generators;

[Generator]
public class CombineGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var sourceCode = CompilationUnit()
            .WithMembers(SingletonList<MemberDeclarationSyntax>(NamespaceDeclaration(QualifiedName(IdentifierName("Moq"), IdentifierName("AutoMock")))
                .WithMembers(SingletonList<MemberDeclarationSyntax>(ClassDeclaration("AutoMocker")
                    .WithModifiers(TokenList(Token(PartialKeyword)))
                    .WithMembers(List(Enumerable.Range(1, 10).Select(Combine)))))))
            .NormalizeWhitespace()
            .ToFullString();

        context.AddSource(nameof(CombineGenerator), sourceCode);
    }

    private MemberDeclarationSyntax Combine(int count)
    {
        return MethodDeclaration(PredefinedType(Token(VoidKeyword)), "Combine")
            .WithModifiers(TokenList(Token(TriviaList(Trivia(Documentation)), PublicKeyword, TriviaList())))
            .WithTypeParameterList(TypeParameterList(SeparatedList(Enumerable.Range(0, count + 1).Select(type))))
            .WithExpressionBody(ArrowExpressionClause(
                InvocationExpression(IdentifierName("Combine"))
                    .WithArgumentList(ArgumentList(SeparatedList(Enumerable.Range(0, count + 1).Select(argument))))))
            .WithSemicolonToken(Token(SemicolonToken))
            .WithTrailingTrivia(LineFeed);

        static string identifier(int index) => index is 0 ? "TService" : $"TAsWellAs{index}";
        static TypeParameterSyntax type(int index) => TypeParameter(identifier(index));
        static ArgumentSyntax argument(int index) => Argument(TypeOfExpression(IdentifierName(identifier(index))));
    }

    private DocumentationCommentTriviaSyntax Documentation { get; } = DocumentationComment(
        XmlText(" "),
        XmlSummaryElement(
            new[]
            {
                    "Combines all given types so that they are mocked by the same",
                    @"mock. Some IoC containers call this ""Forwarding"" one type to",
                    "other interfaces. In the end, this just means that all given",
                    "types will be implemnted by the same instance.",
            }.SelectMany(text => new[] { XmlNewLine(Environment.NewLine), XmlText($" {text}") })
            .Concat(new[] { XmlNewLine(Environment.NewLine), XmlText(" ") })
            .ToArray()
        ),
        XmlText($"{Environment.NewLine}        "));
}
