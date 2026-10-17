using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Moq.AutoMocker.Generators;

/// <summary>
/// Replaces <c>mocker.Use(mocker.CreateInstance&lt;T&gt;())</c> with <c>mocker.With&lt;T&gt;()</c>.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCreateInstanceCodeFixProvider)), Shared]
public sealed class UseCreateInstanceCodeFixProvider : CodeFixProvider
{
    private const string Title = "Use With";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(Diagnostics.PreferWith.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        Diagnostic diagnostic = context.Diagnostics[0];
        SyntaxNode? node = root.FindNode(diagnostic.Location.SourceSpan);
        InvocationExpressionSyntax? invocation = node.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().FirstOrDefault();
        if (invocation is null || CreateReplacement(invocation) is null) return;

        context.RegisterCodeFix(
            CodeAction.Create(Title, ct => ApplyAsync(context.Document, invocation, ct), equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> ApplyAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        InvocationExpressionSyntax? replacement = CreateReplacement(invocation);
        if (root is null || replacement is null) return document;
        return document.WithSyntaxRoot(root.ReplaceNode(invocation, replacement));
    }

    private static InvocationExpressionSyntax? CreateReplacement(InvocationExpressionSyntax use)
    {
        if (use.Expression is not MemberAccessExpressionSyntax useAccess) return null;

        var arguments = use.ArgumentList.Arguments;
        if (arguments.Count == 0) return null;
        if (UnwrapParentheses(arguments[arguments.Count - 1].Expression) is not InvocationExpressionSyntax create
            || create.Expression is not MemberAccessExpressionSyntax createAccess)
        {
            return null;
        }

        SimpleNameSyntax newName;
        ArgumentListSyntax newArguments;
        if (arguments.Count == 1)
        {
            if (createAccess.Name is not GenericNameSyntax createName) return null;
            TypeSyntax implementation = createName.TypeArgumentList.Arguments[0];
            var typeArguments = SyntaxFactory.SeparatedList<TypeSyntax>();
            if (useAccess.Name is GenericNameSyntax useName
                && !SyntaxFactory.AreEquivalent(useName.TypeArgumentList.Arguments[0], implementation))
            {
                typeArguments = typeArguments.Add(useName.TypeArgumentList.Arguments[0]);
            }
            typeArguments = typeArguments.Add(implementation);
            newName = SyntaxFactory.GenericName(SyntaxFactory.Identifier("With"), SyntaxFactory.TypeArgumentList(typeArguments));
            newArguments = SyntaxFactory.ArgumentList();
        }
        else
        {
            ExpressionSyntax service = arguments[0].Expression;
            ExpressionSyntax implementation = create.ArgumentList.Arguments[0].Expression;
            newName = SyntaxFactory.IdentifierName("With");
            newArguments = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(
                SyntaxFactory.AreEquivalent(service, implementation)
                    ? new[] { SyntaxFactory.Argument(implementation) }
                    : new[] { SyntaxFactory.Argument(service), SyntaxFactory.Argument(implementation) }));
        }

        return use
            .WithExpression(useAccess.WithName(newName.WithTriviaFrom(useAccess.Name)))
            .WithArgumentList(newArguments.WithTriviaFrom(use.ArgumentList));
    }

    private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
        {
            expression = parenthesized.Expression;
        }
        return expression;
    }
}
