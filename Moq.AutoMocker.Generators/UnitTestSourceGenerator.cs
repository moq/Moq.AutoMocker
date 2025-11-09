using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Moq.AutoMocker.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class UnitTestSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Gather info for all annotated command methods (starting from method declarations with at least one attribute)
        IncrementalValuesProvider<(GeneratorTargetClass? targetClass, ImmutableArray<Diagnostic> diagnostics)> commandInfoWithErrors =
            context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Moq.AutoMock.ConstructorTestsAttribute",
                static (node, _) => node is ClassDeclarationSyntax,
                static (context, token) =>
                {
                    var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
                    
                    if (((CSharpCompilation)context.SemanticModel.Compilation).LanguageVersion < LanguageVersion.CSharp5)
                    {
                        return (null, diagnostics.ToImmutable());
                    }

                    var constructorTestsAttribute = context.Attributes[0];

                    INamedTypeSymbol? sutType = GetTargetSutType(constructorTestsAttribute);
                    if (sutType is null)
                    {
                        // Report diagnostic: must specify target type
                        INamedTypeSymbol testClassSymbol = (INamedTypeSymbol)context.TargetSymbol;
                        Diagnostic diagnostic = Diagnostics.MustSpecifyTargetType.Create(
                            constructorTestsAttribute.ApplicationSyntaxReference?.GetSyntax(token).GetLocation(),
                            testClassSymbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                        diagnostics.Add(diagnostic);
                        return (null, diagnostics.ToImmutable());
                    }
                    token.ThrowIfCancellationRequested();

                    INamedTypeSymbol testClassSymbol2 = (INamedTypeSymbol)context.TargetSymbol;
                    
                    // Check if class is partial
                    var classSyntax = context.TargetNode as ClassDeclarationSyntax;
                    if (classSyntax != null && !classSyntax.Modifiers.Any(m => m.IsKind(PartialKeyword)))
                    {
                        Diagnostic diagnostic = Diagnostics.TestClassesMustBePartial.Create(
                            classSyntax.GetLocation(),
                            testClassSymbol2.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));
                        diagnostics.Add(diagnostic);
                        return (null, diagnostics.ToImmutable());
                    }

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

                    string testClassName = testClassSymbol2.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                    bool skipNullableParameters = GetSkipNullableParameters(constructorTestsAttribute);
                    token.ThrowIfCancellationRequested();
                    
                    var targetClass = new GeneratorTargetClass
                    {
                        Namespace = testClassSymbol2.ContainingNamespace.ToDisplayString(),
                        TestClassName = testClassName,
                        Sut = sut,
                        SkipNullableParameters = skipNullableParameters
                    };
                    
                    return ((GeneratorTargetClass?)targetClass, diagnostics.ToImmutable());
                });

        // Output the diagnostics
        context.RegisterSourceOutput(commandInfoWithErrors, static (context, item) =>
        {
            foreach (var diagnostic in item.diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }
        });

        // Get the filtered sequence to enable caching - only items with valid target classes
        IncrementalValuesProvider<GeneratorTargetClass> commandInfo =
            commandInfoWithErrors
            .Select(static (item, _) => item.targetClass)
            .Where(static item => item is not null)!;

        // Combine with compilation to get testing framework info
        IncrementalValueProvider<(Compilation, ImmutableArray<GeneratorTargetClass>)> compilationAndClasses =
            context.CompilationProvider.Combine(commandInfo.Collect());

        // Generate the commands
        context.RegisterSourceOutput(compilationAndClasses, static (context, source) =>
        {
            var (compilation, classes) = source;
            var testingFramework = GetTestingFramework(compilation.ReferencedAssemblyNames);

            foreach (var item in classes)
            {
                ImmutableArray<MemberDeclarationSyntax> memberDeclarations = GetMembers(item, testingFramework);
                CompilationUnitSyntax compilationUnit = GetCompilationUnit(item, memberDeclarations);

                context.AddSource($"{item.TestClassName}.g.cs", compilationUnit.NormalizeWhitespace().ToFullString());
            }
        });
    }

    private static ImmutableArray<MemberDeclarationSyntax> GetMembers(GeneratorTargetClass testClass, TargetTestingFramework testingFramework)
    {
        var members = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>();
        HashSet<string> testNames = new();

        // Add the AutoMockerTestSetup partial method
        members.Add(
            MethodDeclaration(
                PredefinedType(Token(VoidKeyword)),
                Identifier("AutoMockerTestSetup"))
            .WithModifiers(TokenList(Token(PartialKeyword)))
            .WithParameterList(ParameterList(SeparatedList(new[]
            {
                Parameter(Identifier("mocker"))
                    .WithType(ParseTypeName("Moq.AutoMock.AutoMocker")),
                Parameter(Identifier("testName"))
                    .WithType(PredefinedType(Token(StringKeyword)))
            })))
            .WithSemicolonToken(Token(SemicolonToken)));

        foreach (NullConstructorParameterTest test in testClass.Sut?.NullConstructorParameterTests ?? Enumerable.Empty<NullConstructorParameterTest>())
        {
            if (test.Parameters?[test.NullParameterIndex].IsValueType == true)
            {
                continue;
            }
            if (testClass.SkipNullableParameters && test.Parameters?[test.NullParameterIndex].IsNullable == true)
            {
                continue;
            }

            string testName = "";
            foreach (var name in TestNameBuilder.CreateTestName(testClass, test))
            {
                if (testNames.Add(name))
                {
                    testName = name;
                    break;
                }
            }

            // Add setup partial method for this test
            members.Add(
                MethodDeclaration(
                    PredefinedType(Token(VoidKeyword)),
                    Identifier($"{testName}Setup"))
                .WithModifiers(TokenList(Token(PartialKeyword)))
                .WithParameterList(ParameterList(SingletonSeparatedList(
                    Parameter(Identifier("mocker"))
                        .WithType(ParseTypeName("Moq.AutoMock.AutoMocker")))))
                .WithSemicolonToken(Token(SemicolonToken)));

            // Add the test method
            members.Add(CreateTestMethod(testClass, test, testName, testingFramework));
        }

        return members.ToImmutable();
    }

    private static MethodDeclarationSyntax CreateTestMethod(
        GeneratorTargetClass testClass,
        NullConstructorParameterTest test,
        string testName,
        TargetTestingFramework testingFramework)
    {
        var attributes = new List<AttributeListSyntax>();

        // Add test attribute based on framework
        switch (testingFramework)
        {
            case TargetTestingFramework.MSTest:
                attributes.Add(AttributeList(SingletonSeparatedList(
                    Attribute(ParseName("global::Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod")))));
                break;
            case TargetTestingFramework.XUnit:
                attributes.Add(AttributeList(SingletonSeparatedList(
                    Attribute(ParseName("global::Xunit.Fact")))));
                break;
            case TargetTestingFramework.NUnit:
                attributes.Add(AttributeList(SingletonSeparatedList(
                    Attribute(ParseName("global::NUnit.Framework.Test")))));
                break;
        }

        var statements = new List<StatementSyntax>
        {
            // Moq.AutoMock.AutoMocker mocker = new Moq.AutoMock.AutoMocker();
            LocalDeclarationStatement(
                VariableDeclaration(ParseTypeName("Moq.AutoMock.AutoMocker"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("mocker"))
                    .WithInitializer(EqualsValueClause(
                        ObjectCreationExpression(ParseTypeName("Moq.AutoMock.AutoMocker"))
                        .WithArgumentList(ArgumentList())))))),

            // AutoMockerTestSetup(mocker, "testName");
            ExpressionStatement(
                InvocationExpression(IdentifierName("AutoMockerTestSetup"))
                .WithArgumentList(ArgumentList(SeparatedList(new[]
                {
                    Argument(IdentifierName("mocker")),
                    Argument(LiteralExpression(StringLiteralExpression, Literal(testName)))
                })))),

            // {testName}Setup(mocker);
            ExpressionStatement(
                InvocationExpression(IdentifierName($"{testName}Setup"))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                    Argument(IdentifierName("mocker"))))))
        };

        // Build the using statement body
        var usingStatements = new List<StatementSyntax>();

        // Add Get<> calls for non-null parameters
        for (int i = 0; i < test.Parameters?.Count; i++)
        {
            if (i == test.NullParameterIndex) continue;
            Parameter parameter = test.Parameters[i];

            usingStatements.Add(
                LocalDeclarationStatement(
                    VariableDeclaration(IdentifierName("var"))
                    .WithVariables(SingletonSeparatedList(
                        VariableDeclarator(Identifier(parameter.Name))
                        .WithInitializer(EqualsValueClause(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SimpleMemberAccessExpression,
                                    IdentifierName("mocker"),
                                    GenericName(Identifier("Get"))
                                    .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(
                                        ParseTypeName(parameter.ParameterType))))))
                            .WithArgumentList(ArgumentList())))))));
        }

        // Create constructor invocation with parameter names
        var parameterNames = GetParameterNames(test).ToList();
        var constructorArguments = SeparatedList(
            parameterNames.Select(pName =>
                Argument(pName.StartsWith("default(")
                    ? DefaultExpression(ParseTypeName(pName.Substring(8, pName.Length - 9)))
                    : IdentifierName(pName))));

        var constructorInvocation = 
            AssignmentExpression(
                SimpleAssignmentExpression,
                IdentifierName("_"),
                ObjectCreationExpression(ParseTypeName(testClass.Sut!.FullName!))
                    .WithArgumentList(ArgumentList(constructorArguments)));

        // Add assertion based on framework
        switch (testingFramework)
        {
            case TargetTestingFramework.MSTest:
                {
                    var lambda = ParenthesizedLambdaExpression()
                        .WithParameterList(ParameterList())
                        .WithExpressionBody(constructorInvocation);

                    var throwsInvocation = InvocationExpression(
                            MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                ParseName("global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert"),
                                GenericName(Identifier("Throws"))
                                .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(
                                    ParseTypeName("System.ArgumentNullException"))))))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(lambda))));

                    usingStatements.Add(
                        LocalDeclarationStatement(
                            VariableDeclaration(ParseTypeName("System.ArgumentNullException"))
                            .WithVariables(SingletonSeparatedList(
                                VariableDeclarator(Identifier("ex"))
                                .WithInitializer(EqualsValueClause(throwsInvocation))))));

                    usingStatements.Add(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SimpleMemberAccessExpression,
                                    ParseName("global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert"),
                                    IdentifierName("AreEqual")))
                        .WithArgumentList(ArgumentList(SeparatedList(new[]
                        {
                            Argument(LiteralExpression(StringLiteralExpression, Literal(test.NullParameterName!))),
                            Argument(MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                IdentifierName("ex"),
                                IdentifierName("ParamName")))
                        })))));

                }
                break;

            case TargetTestingFramework.XUnit:
                {
                    var lambda = ParenthesizedLambdaExpression()
                        .WithParameterList(ParameterList())
                        .WithExpressionBody(constructorInvocation);

                    var throwsInvocation = InvocationExpression(
                            MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                ParseName("global::Xunit.Assert"),
                                GenericName(Identifier("Throws"))
                                .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(
                                    ParseTypeName("System.ArgumentNullException"))))))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(lambda))));

                    usingStatements.Add(
                        LocalDeclarationStatement(
                            VariableDeclaration(ParseTypeName("System.ArgumentNullException"))
                            .WithVariables(SingletonSeparatedList(
                                VariableDeclarator(Identifier("ex"))
                                .WithInitializer(EqualsValueClause(throwsInvocation))))));

                    usingStatements.Add(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SimpleMemberAccessExpression,
                                    ParseName("global::Xunit.Assert"),
                                    IdentifierName("Equal")))
                        .WithArgumentList(ArgumentList(SeparatedList(new[]
                        {
                            Argument(LiteralExpression(StringLiteralExpression, Literal(test.NullParameterName!))),
                            Argument(MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                IdentifierName("ex"),
                                IdentifierName("ParamName")))
                        })))));
                }
                break;

            case TargetTestingFramework.NUnit:
                {
                    var lambda = ParenthesizedLambdaExpression()
                        .WithParameterList(ParameterList())
                        .WithExpressionBody(constructorInvocation);

                    var throwsInvocation = InvocationExpression(
                            MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                ParseName("global::NUnit.Framework.Assert"),
                                GenericName(Identifier("Throws"))
                                .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(
                                    ParseTypeName("System.ArgumentNullException"))))))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(lambda))));

                    usingStatements.Add(
                        LocalDeclarationStatement(
                            VariableDeclaration(ParseTypeName("System.ArgumentNullException"))
                            .WithVariables(SingletonSeparatedList(
                                VariableDeclarator(Identifier("ex"))
                                .WithInitializer(EqualsValueClause(throwsInvocation))))));

                    usingStatements.Add(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SimpleMemberAccessExpression,
                                    ParseName("global::NUnit.Framework.Assert"),
                                    IdentifierName("That")))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(
                            Argument(BinaryExpression(
                                EqualsExpression,
                                LiteralExpression(StringLiteralExpression, Literal(test.NullParameterName!)),
                                MemberAccessExpression(
                                    SimpleMemberAccessExpression,
                                    IdentifierName("ex"),
                                    IdentifierName("ParamName")))))))));
                }
                break;
        }

        // using(System.IDisposable __mockerDisposable = mocker.AsDisposable()) { ... }
        statements.Add(
            UsingStatement(Block(usingStatements))
            .WithDeclaration(VariableDeclaration(ParseTypeName("System.IDisposable"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("__mockerDisposable"))
                    .WithInitializer(EqualsValueClause(
                        InvocationExpression(
                            MemberAccessExpression(
                                SimpleMemberAccessExpression,
                                IdentifierName("mocker"),
                                IdentifierName("AsDisposable")))
                        .WithArgumentList(ArgumentList())))))));


        return MethodDeclaration(
            PredefinedType(Token(VoidKeyword)),
            Identifier(testName))
            .WithAttributeLists(List(attributes))
            .WithModifiers(TokenList(Token(PublicKeyword)))
            .WithBody(Block(statements));
    }

    private static IEnumerable<string> GetParameterNames(NullConstructorParameterTest test)
    {
        for (int i = 0; i < test.Parameters?.Count; i++)
        {
            yield return i == test.NullParameterIndex
                ? $"default({test.Parameters[i].ParameterType})"
                : test.Parameters[i].Name;
        }
    }

    private static CompilationUnitSyntax GetCompilationUnit(
        GeneratorTargetClass testClass,
        ImmutableArray<MemberDeclarationSyntax> memberDeclarations)
    {
        return CompilationUnit()
            .WithMembers(SingletonList<MemberDeclarationSyntax>(
                FileScopedNamespaceDeclaration(ParseName(testClass.Namespace!))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(
                        ClassDeclaration(testClass.TestClassName!)
                            .WithModifiers(TokenList(Token(PartialKeyword)))
                            .WithMembers(List(memberDeclarations))))));
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
        static INamedTypeSymbol? GetSutClass(TypedConstant arg)
        {
            // Check if this is a typeof() argument
            if (arg.Kind == TypedConstantKind.Type &&
                arg.Value is INamedTypeSymbol targetTypeSymbol)
            {
                return targetTypeSymbol;
            }
            return null;
        }

        // Check constructor arguments (e.g., [ConstructorTests(typeof(MyClass))])
        foreach (var arg in constructorTestsAttribute.ConstructorArguments)
        {
            if (GetSutClass(arg) is { } sutClass)
            {
                return sutClass;
            }
        }

        // Check named arguments (e.g., [ConstructorTests(TargetType = typeof(MyClass))])
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

    private static TargetTestingFramework GetTestingFramework(IEnumerable<AssemblyIdentity> assemblies)
    {
        foreach (AssemblyIdentity assembly in assemblies)
        {
            if (assembly.Name.StartsWith("Microsoft.VisualStudio.TestPlatform.TestFramework"))
            {
                return TargetTestingFramework.MSTest;
            }
            if (assembly.Name.StartsWith("nunit."))
            {
                return TargetTestingFramework.NUnit;
            }
            if (assembly.Name.StartsWith("xunit."))
            {
                return TargetTestingFramework.XUnit;
            }
        }
        return TargetTestingFramework.Unknown;
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
 //           {
 //               if (test.Parameters?[test.NullParameterIndex].IsValueType == true)
 //               {
 //                   continue;
 //               }
 //               if (testClass.SkipNullableParameters && test.Parameters?[test.NullParameterIndex].IsNullable == true)
 //               {
 //                   continue;
 //               }
 //               string testName = "";
 //               foreach (var name in TestNameBuilder.CreateTestName(testClass, test))
 //               {
 //                   if (testNames.Add(name))
 //                   {
 //                       testName = name;
 //                       break;
 //                   }
 //               }
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
 //           }
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
