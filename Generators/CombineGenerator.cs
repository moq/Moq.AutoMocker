using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Generators;

[Generator(LanguageNames.CSharp)]
public sealed class CombineGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static (context) =>
        {
            CompilationUnitSyntax compilationUnit = GetCompilationUnit();
            context.AddSource($"{nameof(CombineGenerator)}.g.cs", compilationUnit.NormalizeWhitespace().ToFullString());
        });
    }

    private static CompilationUnitSyntax GetCompilationUnit()
    {
        var methods = Enumerable.Range(1, 10).Select(CreateCombineMethod).ToArray();

        return CompilationUnit()
            .WithMembers(SingletonList<MemberDeclarationSyntax>(
                FileScopedNamespaceDeclaration(QualifiedName(IdentifierName("Moq"), IdentifierName("AutoMock")))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                        ClassDeclaration("AutoMocker")
                            .WithModifiers(TokenList(Token(PartialKeyword)))
                            .WithMembers(List<MemberDeclarationSyntax>(methods))))));
    }

    private static MemberDeclarationSyntax CreateCombineMethod(int count)
    {
        return MethodDeclaration(PredefinedType(Token(VoidKeyword)), "Combine")
            .WithModifiers(TokenList(Token(TriviaList(Trivia(GetDocumentation())), PublicKeyword, TriviaList())))
            .WithTypeParameterList(TypeParameterList(SeparatedList(Enumerable.Range(0, count + 1).Select(CreateTypeParameter))))
            .WithExpressionBody(ArrowExpressionClause(
                InvocationExpression(IdentifierName("Combine"))
                    .WithArgumentList(ArgumentList(SeparatedList(Enumerable.Range(0, count + 1).Select(CreateArgument))))))
            .WithSemicolonToken(Token(SemicolonToken))
            .WithTrailingTrivia(LineFeed);

        static string GetIdentifier(int index) => index is 0 ? "TService" : $"TAsWellAs{index}";
        static TypeParameterSyntax CreateTypeParameter(int index) => TypeParameter(GetIdentifier(index));
        static ArgumentSyntax CreateArgument(int index) => Argument(TypeOfExpression(IdentifierName(GetIdentifier(index))));
    }

    private static DocumentationCommentTriviaSyntax GetDocumentation()
    {
        return DocumentationComment(
            XmlText(" "),
            XmlSummaryElement(
                new[]
                {
                    "Combines all given types so that they are mocked by the same",
                    @"mock. Some IoC containers call this ""Forwarding"" one type to",
                    "other interfaces. In the end, this just means that all given",
                    "types will be implemented by the same instance.",
                }.SelectMany(text => new[] { XmlNewLine(NewLine), XmlText($" {text}") })
                .Concat([XmlNewLine(NewLine), XmlText(" ")])
                .ToArray()
            ),
            XmlText($"{NewLine}        "));
    }

    //A new line that will respect the checked out state of auto.crlf
    private const string NewLine = @"
";
}
