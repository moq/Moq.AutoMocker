using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Generators
{
    [Generator]
    public class CombineGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var sourceCode = CompilationUnit()
                .WithUsings(List(new[]
                {
                    typeof(GeneratedCodeAttribute),
                    typeof(CompilerGeneratedAttribute),
                    typeof(ExcludeFromCodeCoverageAttribute),
                }.Distinct().Select(x => UsingDirective(IdentifierName(x.Namespace)))))
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
                .WithModifiers(TokenList(Token(PublicKeyword)))
                .WithAttributeLists(List(attributes()))
                .WithTypeParameterList(TypeParameterList(SeparatedList(Enumerable.Range(0, count + 1).Select(type))))
                .WithExpressionBody(ArrowExpressionClause(
                    InvocationExpression(IdentifierName("Combine"))
                        .WithArgumentList(ArgumentList(SeparatedList(Enumerable.Range(0, count + 1).Select(argument))))))
                .WithSemicolonToken(Token(SemicolonToken))
                .WithTrailingTrivia(LineFeed);

            static string identifier(int index) => index is 0 ? "TService" : $"TAsWellAs{index}";
            static TypeParameterSyntax type(int index) => TypeParameter(identifier(index));
            static ArgumentSyntax argument(int index) => Argument(TypeOfExpression(IdentifierName(identifier(index))));
            IEnumerable<AttributeListSyntax> attributes()
            {
                var array = new[]
                {
                    Attribute(IdentifierName(nameof(GeneratedCodeAttribute)), AttributeArgumentList(SeparatedList(new []
                    {
                        AttributeArgument(LiteralExpression(StringLiteralExpression, Literal(nameof(CombineGenerator)))),
                        AttributeArgument(LiteralExpression(StringLiteralExpression, Literal(Assembly.GetExecutingAssembly().GetName().Version.ToString()))),
                    }))),
                    Attribute(IdentifierName(nameof(CompilerGeneratedAttribute))),
                    Attribute(IdentifierName(nameof(ExcludeFromCodeCoverageAttribute))),
                }.Select(a => AttributeList(SeparatedList(new[]{a})))
                .ToList();
                
                array[0] = array[0].WithLeadingTrivia(Trivia(Documentation));
                return array;
            }
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
}
