using System.Collections.Immutable;
using System.Text;
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
        // Check if Moq.AutoMock is referenced
        IncrementalValueProvider<bool> referencesAutoMock = context.CompilationProvider
            .Select(static (compilation, _) => ReferencesAutoMock(compilation.ReferencedAssemblyNames));

        // Report diagnostic if AutoMock is not referenced
        context.RegisterSourceOutput(referencesAutoMock, static (context, hasReference) =>
        {
            if (!hasReference)
            {
                context.ReportDiagnostic(Diagnostics.MustReferenceAutoMock.Create());
            }
        });

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
                string generatedCode = GenerateTestClass(item, testingFramework);
                context.AddSource($"{item.TestClassName}.g.cs", generatedCode);
            }
        });
    }

    private static string GenerateTestClass(GeneratorTargetClass testClass, TargetTestingFramework testingFramework)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"namespace {testClass.Namespace}");
        builder.AppendLine("{");
        builder.AppendLine($"    partial class {testClass.TestClassName}");
        builder.AppendLine("    {");
        builder.AppendLine("        partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName);");

        HashSet<string> testNames = [];

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

            builder.AppendLine();
            builder.AppendLine($"        partial void {testName}Setup(Moq.AutoMock.AutoMocker mocker);");
            builder.AppendLine();

            switch (testingFramework)
            {
                case TargetTestingFramework.MSTest:
                    builder.AppendLine("        [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]");
                    break;
                case TargetTestingFramework.XUnit:
                    builder.AppendLine("        [global::Xunit.Fact]");
                    break;
                case TargetTestingFramework.NUnit:
                    builder.AppendLine("        [global::NUnit.Framework.Test]");
                    break;
                case TargetTestingFramework.TUnit:
                    builder.AppendLine("        [global::TUnit.Core.Test]");
                    break;
            }

            string methodSignature = testingFramework == TargetTestingFramework.TUnit
                ? $"        public async System.Threading.Tasks.Task {testName}()"
                : $"        public void {testName}()";
            
            builder.AppendLine(methodSignature);
            builder.AppendLine("        {");
            builder.AppendLine("            Moq.AutoMock.AutoMocker mocker = new Moq.AutoMock.AutoMocker();");
            builder.AppendLine($"            AutoMockerTestSetup(mocker, \"{testName}\");");
            builder.AppendLine($"            {testName}Setup(mocker);");
            builder.AppendLine("            using(System.IDisposable __mockerDisposable = mocker.AsDisposable())");
            builder.AppendLine("            {");

            const string indent = "                ";
            for (int i = 0; i < test.Parameters?.Count; i++)
            {
                if (i == test.NullParameterIndex) continue;
                Parameter parameter = test.Parameters[i];
                builder.AppendLine($"{indent}var {parameter.Name} = mocker.Get<{parameter.ParameterType}>();");
            }

            string constructorInvocation = $"_ = new {testClass.Sut!.FullName}({string.Join(", ", GetParameterNames(test))})";

            switch (testingFramework)
            {
                case TargetTestingFramework.MSTest:
                    builder.AppendLine($"{indent}System.ArgumentNullException ex = global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Throws<System.ArgumentNullException>(() => {constructorInvocation});");
                    builder.AppendLine($"{indent}global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(\"{test.NullParameterName}\", ex.ParamName);");
                    break;

                case TargetTestingFramework.XUnit:
                    builder.AppendLine($"{indent}System.ArgumentNullException ex = global::Xunit.Assert.Throws<System.ArgumentNullException>(() => {constructorInvocation});");
                    builder.AppendLine($"{indent}global::Xunit.Assert.Equal(\"{test.NullParameterName}\", ex.ParamName);");
                    break;

                case TargetTestingFramework.NUnit:
                    builder.AppendLine($"{indent}System.ArgumentNullException ex = global::NUnit.Framework.Assert.Throws<System.ArgumentNullException>(() => {constructorInvocation});");
                    builder.AppendLine($"{indent}global::NUnit.Framework.Assert.That(\"{test.NullParameterName}\" == ex.ParamName);");
                    break;

                case TargetTestingFramework.TUnit:
                    builder.AppendLine($"{indent}System.ArgumentNullException ex = global::TUnit.Assertions.Assert.Throws<System.ArgumentNullException>(() => {constructorInvocation});");
                    builder.AppendLine($"{indent}await global::TUnit.Assertions.Assert.That(ex.ParamName).IsEqualTo(\"{test.NullParameterName}\");");
                    break;
            }

            builder.AppendLine("            }");
            builder.AppendLine("        }");
            builder.AppendLine();
        }

        if (testingFramework == TargetTestingFramework.TUnit && testNames.Count > 0)
        {
            builder.AppendLine("""
                        public static void AddAutoMockerConstuctorTests(DynamicTestBuilderContext context)
                        {
                """);
            foreach (var testName in testNames)
            {
                builder.AppendLine(
                    $$"""
                                context.AddTest(new DynamicTest<{{testClass.TestClassName}}>
                                {
                                    TestMethod = @class => @class.{{testName}}()
                                });
                    """);
            }
            builder.AppendLine("        }");
        }

        // Only add blank line before closing brace if there are no tests
        if (testNames.Count == 0)
        {
            builder.AppendLine();
        }
        
        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
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

    private static bool GetSkipNullableParameters(AttributeData constructorTestsAttribute)
    {
        static bool? GetBehavior(TypedConstant arg)
        {
            if (arg.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Moq.AutoMock.TestGenerationBehavior")
            {
                // Check if the enum value is IgnoreNullableParameters (value 1)
                if (arg.Value is int enumValue)
                {
                    return enumValue == 1; // 1 = IgnoreNullableParameters
                }
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
            if (assembly.Name.Equals("TUnit.Core", StringComparison.OrdinalIgnoreCase))
            {
                return TargetTestingFramework.TUnit;
            }
        }
        return TargetTestingFramework.Unknown;
    }

    private static bool ReferencesAutoMock(IEnumerable<AssemblyIdentity> assemblies)
    {
        foreach (AssemblyIdentity assembly in assemblies)
        {
            if (assembly.Name.Equals(AutoMock.AssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}
