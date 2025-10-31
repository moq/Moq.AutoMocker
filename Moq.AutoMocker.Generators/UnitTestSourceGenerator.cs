using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Moq.AutoMocker.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class UnitTestSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        IncrementalValuesProvider<GeneratorTargetClass> commandInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Moq.AutoMock.ConstructorTestsAttribute",
                static (node, _) => node is ClassDeclarationSyntax classDeclaration,
                static (context, token) =>
                {
                    if (((CSharpCompilation)context.SemanticModel.Compilation).LanguageVersion < LanguageVersion.CSharp5)
                    {
                        return GeneratorTargetClass.Empty;
                    }

                    var constructorTestsAttribute = context.Attributes[0];

                    INamedTypeSymbol? sutType = GetTargetSutType(constructorTestsAttribute);
                    if (sutType is null)
                    {
                        return GeneratorTargetClass.Empty;
                    }
                    token.ThrowIfCancellationRequested();

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
                        token.ThrowIfCancellationRequested();
                    }

                    INamedTypeSymbol testClassSymbol = (INamedTypeSymbol)context.TargetSymbol;
                    string testClassName = testClassSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                    bool skipNullableParameters = GetSkipNullableParameters(constructorTestsAttribute);
                    token.ThrowIfCancellationRequested();
                    return new()
                    {
                        Namespace = testClassSymbol.ContainingNamespace.ToDisplayString(),
                        TestClassName = testClassName,
                        Sut = sut,
                        SkipNullableParameters = skipNullableParameters
                    };
                })
            .Where(static item => item is not null)!;

        //TODO: Add
        // Output the diagnostics
        //context.ReportDiagnostics(commandInfoWithErrors.Select(static (item, _) => item.Info.Errors));

        // Get the filtered sequence to enable caching
        IncrementalValuesProvider<GeneratorTargetClass> commandInfo =
            commandInfoWithErrors
            .Where(static item => item.Sut is not null)!;

        // Generate the commands
        context.RegisterSourceOutput(commandInfo, static (context, item) =>
        {
            ImmutableArray<MemberDeclarationSyntax> memberDeclarations = GetMembers(item.Sut);
            CompilationUnitSyntax compilationUnit = GetCompilationUnit(memberDeclarations);

            context.AddSource($"{item.TestClassName}.g.cs", compilationUnit);
        });
    }

    private static ImmutableArray<MemberDeclarationSyntax> GetMembers(SutClass sut)
    {
        return ImmutableArray<MemberDeclarationSyntax>.Empty;
    }

    public CompilationUnitSyntax GetCompilationUnit(
       ImmutableArray<MemberDeclarationSyntax> memberDeclarations,
       BaseListSyntax? baseList = null)
    {
        // Create the partial type declaration with the given member declarations.
        // This code produces a class declaration as follows:
        //
        // /// <inheritdoc/>
        // partial <TYPE_KIND> TYPE_NAME>
        // {
        //     <MEMBERS>
        // }
        TypeDeclarationSyntax typeDeclarationSyntax =
            Hierarchy[0].GetSyntax()
            .AddModifiers(Token(TriviaList(Comment("/// <inheritdoc/>")), SyntaxKind.PartialKeyword, TriviaList()))
            .AddMembers(memberDeclarations.ToArray());

        // Add the base list, if present
        if (baseList is not null)
        {
            typeDeclarationSyntax = typeDeclarationSyntax.WithBaseList(baseList);
        }

        // Add all parent types in ascending order, if any
        foreach (TypeInfo parentType in Hierarchy.AsSpan().Slice(1))
        {
            typeDeclarationSyntax =
                parentType.GetSyntax()
                .AddModifiers(Token(TriviaList(Comment("/// <inheritdoc/>")), SyntaxKind.PartialKeyword, TriviaList()))
                .AddMembers(typeDeclarationSyntax);
        }

        // Prepare the leading trivia for the generated compilation unit.
        // This will produce code as follows:
        //
        // <auto-generated/>
        // #pragma warning disable
        // #nullable enable
        SyntaxTriviaList syntaxTriviaList = TriviaList(
            Comment("// <auto-generated/>"),
            Trivia(PragmaWarningDirectiveTrivia(Token(SyntaxKind.DisableKeyword), true)),
            Trivia(NullableDirectiveTrivia(Token(SyntaxKind.EnableKeyword), true)));

        if (Namespace is "")
        {
            // Special case for top level types with no namespace: we need to re-add the
            // inheritdoc XML comment, as otherwise the call below would remove it.
            syntaxTriviaList = syntaxTriviaList.Add(Comment("/// <inheritdoc/>"));

            // If there is no namespace, attach the pragma directly to the declared type,
            // and skip the namespace declaration. This will produce code as follows:
            //
            // <SYNTAX_TRIVIA>
            // <TYPE_HIERARCHY>
            return
                CompilationUnit()
                .AddMembers(typeDeclarationSyntax.WithLeadingTrivia(syntaxTriviaList))
                .NormalizeWhitespace();
        }

        // Create the compilation unit with disabled warnings, target namespace and generated type.
        // This will produce code as follows:
        //
        // <SYNTAX_TRIVIA>
        // namespace <NAMESPACE>
        // {
        //     <TYPE_HIERARCHY>
        // }
        return
            CompilationUnit().AddMembers(
            NamespaceDeclaration(IdentifierName(Namespace))
            .WithLeadingTrivia(syntaxTriviaList)
            .AddMembers(typeDeclarationSyntax))
            .NormalizeWhitespace();
    }

    private static bool GetSkipNullableParameters(AttributeData constructorTestsAttribute)
    {
        static bool? GetBehavior(TypedConstant arg)
        {
            if (arg.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "Moq.AutoMock.TestGenerationBehavior")
            {
                //TODO Check enum value
                return true;
            }
            return null;
        }

        foreach (var arg in constructorTestsAttribute.ConstructorArguments)
        {
            if (GetBehavior(arg) is { } skipNullableParameter)
            {
                return skipNullableParameter;
            }
        }

        foreach (var arg in constructorTestsAttribute.NamedArguments)
        {
            if (arg.Key == "Behavior" &&
                GetBehavior(arg.Value) is { } skipNullableParameter)
            {
                return skipNullableParameter;
            }
        }
        return false;
    }

    private static INamedTypeSymbol? GetTargetSutType(AttributeData constructorTestsAttribute)
    {
        //TODO: Diagnostics
        /*
         *             if (!(GetTargetType(attribute) is { } targetType) ||
                context.SemanticModel.GetTypeInfo(targetType).Type is not INamedTypeSymbol sutType)
            {
                Diagnostic diagnostic = Diagnostics.MustSpecifyTargetType.Create(attribute.GetLocation(),
                    symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                DiagnosticMessages.Add(diagnostic);
                return;
            }
        */

        static INamedTypeSymbol? GetSutClass(TypedConstant arg)
        {
            if (arg.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "System.Type" &&
                arg.Value is INamedTypeSymbol targetTypeSymbol)
            {
                return targetTypeSymbol;
            }
            return null;
        }

        foreach (var arg in constructorTestsAttribute.ConstructorArguments)
        {
            if (GetSutClass(arg) is { } sutClass)
            {
                return sutClass;
            }
        }

        foreach (var arg in constructorTestsAttribute.NamedArguments)
        {
            if (arg.Key == "TargetType" &&
                GetSutClass(arg.Value) is { } sutClass)
            {
                return sutClass;
            }
        }

        return null;
    }
}

//[Generator]
//public class UnitTestSourceGenerator : ISourceGenerator
//{
//    public void Execute(GeneratorExecutionContext context)
//    {
//        if (context.Compilation.Language is not LanguageNames.CSharp) return;
//
//        if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals(AutoMock.AssemblyName, StringComparison.OrdinalIgnoreCase)))
//        {
//            context.ReportDiagnostic(Diagnostics.MustReferenceAutoMock.Create());
//            return;
//        }
//
//        SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;
//
//        foreach (Diagnostic diagnostic in rx.DiagnosticMessages)
//        {
//            context.ReportDiagnostic(diagnostic);
//        }
//
//        var testingFramework = GetTestingFramework(context.Compilation.ReferencedAssemblyNames);
//
//        foreach (GeneratorTargetClass testClass in rx.TestClasses)
//        {
//            StringBuilder builder = new();
//
//            builder.AppendLine($"namespace {testClass.Namespace}");
//            builder.AppendLine("{");
//
//            builder.AppendLine($"    partial class {testClass.TestClassName}");
//            builder.AppendLine("    {");
//            builder.AppendLine($"        partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName);");
//            builder.AppendLine();
//
//            HashSet<string> testNames = new();
//
//            foreach (NullConstructorParameterTest test in testClass.Sut?.NullConstructorParameterTests ?? Enumerable.Empty<NullConstructorParameterTest>())
//            {
//                if (test.Parameters?[test.NullParameterIndex].IsValueType == true)
//                {
//                    continue;
//                }
//                if (testClass.SkipNullableParameters && test.Parameters?[test.NullParameterIndex].IsNullable == true)
//                {
//                    continue;
//                }
//                string testName = "";
//                foreach (var name in TestNameBuilder.CreateTestName(testClass, test))
//                {
//                    if (testNames.Add(name))
//                    {
//                        testName = name;
//                        break;
//                    }
//                }
//
//                builder.AppendLine($"        partial void {testName}Setup(Moq.AutoMock.AutoMocker mocker);");
//                builder.AppendLine();
//
//                switch (testingFramework)
//                {
//                    case TargetTestingFramework.MSTest:
//                        builder.AppendLine("        [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]");
//                        break;
//                    case TargetTestingFramework.XUnit:
//                        builder.AppendLine("        [global::Xunit.Fact]");
//                        break;
//                    case TargetTestingFramework.NUnit:
//                        builder.AppendLine("        [global::NUnit.Framework.Test]");
//                        break;
//                }
//
//
//                builder.AppendLine($"        public void {testName}()");
//                builder.AppendLine("        {");
//                builder.AppendLine("            Moq.AutoMock.AutoMocker mocker = new Moq.AutoMock.AutoMocker();");
//                builder.AppendLine($"            AutoMockerTestSetup(mocker, \"{testName}\");");
//                builder.AppendLine($"            {testName}Setup(mocker);");
//                builder.AppendLine($"            using(System.IDisposable __mockerDisposable = mocker.AsDisposable())");
//                builder.AppendLine($"            {{");
//
//                const string indent = "                ";
//                for (int i = 0; i < test.Parameters?.Count; i++)
//                {
//                    if (i == test.NullParameterIndex) continue;
//                    Parameter parameter = test.Parameters[i];
//                    builder.AppendLine($"{indent}var {parameter.Name} = mocker.Get<{parameter.ParameterType}>();");
//                }
//
//                string constructorInvocation = $"_ = new {testClass.Sut!.FullName}({string.Join(",", GetParameterNames(test))})";
//
//                switch (testingFramework)
//                {
//                    case TargetTestingFramework.MSTest:
//                        builder.AppendLine($"{indent}System.ArgumentNullException ex = global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Throws<System.ArgumentNullException>(() => {constructorInvocation});");
//                        builder.AppendLine($"{indent}global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(\"{test.NullParameterName}\", ex.ParamName);");
//                        break;
//
//                    case TargetTestingFramework.XUnit:
//                        builder.AppendLine($"{indent}System.ArgumentNullException ex = global::Xunit.Assert.Throws<System.ArgumentNullException>(() => {constructorInvocation});");
//                        builder.AppendLine($"{indent}global::Xunit.Assert.Equal(\"{test.NullParameterName}\", ex.ParamName);");
//                        break;
//
//                    case TargetTestingFramework.NUnit:
//                        builder.AppendLine($"{indent}System.ArgumentNullException ex = global::NUnit.Framework.Assert.Throws<System.ArgumentNullException>(() => {constructorInvocation});");
//                        builder.AppendLine($"{indent}global::NUnit.Framework.Assert.That(\"{test.NullParameterName}\" == ex.ParamName);");
//                        break;
//                }
//                builder.AppendLine("            }");
//                builder.AppendLine("        }");
//                builder.AppendLine();
//            }
//
//            builder.AppendLine("    }");
//            builder.AppendLine("}");
//
//            context.AddSource($"{testClass.TestClassName}.g.cs", builder.ToString());
//
//        }
//
//        static IEnumerable<string> GetParameterNames(NullConstructorParameterTest test)
//        {
//            for (int i = 0; i < test.Parameters?.Count; i++)
//            {
//                yield return i == test.NullParameterIndex
//                    ? $"default({test.Parameters[i].ParameterType})"
//                    : test.Parameters[i].Name;
//            }
//        }
//    }
//
//    public void Initialize(GeneratorInitializationContext context)
//    {
//#if DEBUG
//        if (!System.Diagnostics.Debugger.IsAttached)
//        {
//            //System.Diagnostics.Debugger.Launch();
//        }
//#endif
//        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
//    }
//
//    private static TargetTestingFramework GetTestingFramework(IEnumerable<AssemblyIdentity> assemblies)
//    {
//        foreach (AssemblyIdentity assembly in assemblies)
//        {
//            if (assembly.Name.StartsWith("Microsoft.VisualStudio.TestPlatform.TestFramework"))
//            {
//                return TargetTestingFramework.MSTest;
//            }
//            if (assembly.Name.StartsWith("nunit."))
//            {
//                return TargetTestingFramework.NUnit;
//            }
//            if (assembly.Name.StartsWith("xunit."))
//            {
//                return TargetTestingFramework.XUnit;
//            }
//        }
//        return TargetTestingFramework.Unknown;
//    }
//}
