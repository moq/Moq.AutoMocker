using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMocker.Generators.Tests;

[TestClass]
public class UseOptionsAnalyzerTests
{
    private static async Task<ImmutableArray<Diagnostic>> RunAsync(string source, bool runOptionsGenerator = true)
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var references = trusted.Split(Path.PathSeparator)
            .Select(path => (MetadataReference)MetadataReference.CreateFromFile(path));

        Compilation compilation = CSharpCompilation.Create("Test",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        if (runOptionsGenerator)
        {
            GeneratorDriver driver = CSharpGeneratorDriver.Create(new OptionsExtensionSourceGenerator());
            driver.RunGeneratorsAndUpdateCompilation(compilation, out compilation, out _);
        }

        var withAnalyzers = compilation.WithAnalyzers([new UseOptionsAnalyzer()]);
        var diagnostics = await withAnalyzers.GetAnalyzerDiagnosticsAsync();
        return [.. diagnostics.Where(d => d.Id == Diagnostics.PreferWithOptions.DiagnosticId)];
    }

    private static string Wrap(string body) => $$"""
        using Microsoft.Extensions.Options;
        using Moq.AutoMock;

        public class Settings { public string? Name { get; set; } }

        public class Tests
        {
            public void Run()
            {
                var mocker = new AutoMocker();
                {{body}}
            }
        }
        """;

    [TestMethod]
    public async Task UseWithOptionsCreate_Reports()
    {
        var diagnostics = await RunAsync(Wrap("mocker.Use(Options.Create(new Settings()));"));
        Assert.HasCount(1, diagnostics);
        StringAssert.Contains(diagnostics[0].GetMessage(), "WithOptions<Settings>()");
    }

    [TestMethod]
    public async Task UseWithExplicitIOptionsTypeArgument_Reports()
    {
        var diagnostics = await RunAsync(Wrap("mocker.Use<IOptions<Settings>>(Options.Create(new Settings()));"));
        Assert.HasCount(1, diagnostics);
    }

    [TestMethod]
    public async Task UseWithOptionsWrapper_Reports()
    {
        var diagnostics = await RunAsync(Wrap("mocker.Use(new OptionsWrapper<Settings>(new Settings()));"));
        Assert.HasCount(1, diagnostics);
    }

    [TestMethod]
    public async Task UseWithNonOptions_DoesNotReport()
    {
        var diagnostics = await RunAsync(Wrap("mocker.Use(new Settings());"));
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task WithOptions_DoesNotReport()
    {
        var diagnostics = await RunAsync(Wrap("mocker.WithOptions<Settings>(s => s.Name = \"a\");"));
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task UseWithOptions_WhenExtensionNotGenerated_DoesNotReport()
    {
        var diagnostics = await RunAsync(Wrap("mocker.Use(Options.Create(new Settings()));"), runOptionsGenerator: false);
        Assert.IsEmpty(diagnostics);
    }
}
