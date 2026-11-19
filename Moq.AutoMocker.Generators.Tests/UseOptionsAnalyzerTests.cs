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

        Assert.IsEmpty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));
        var withAnalyzers = compilation.WithAnalyzers([new UseOptionsAnalyzer()]);
        return await withAnalyzers.GetAnalyzerDiagnosticsAsync();
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
        var diagnostics = await RunAsync(Wrap("""
            mocker.Use(Options.Create(new Settings
            {
                Name = "test"
            }));
            """));
        Assert.HasCount(1, diagnostics);
        Assert.AreEqual(Diagnostics.PreferWithOptions.DiagnosticId, diagnostics[0].Id);
        StringAssert.Contains(diagnostics[0].GetMessage(), "WithOptions<Settings>()");
        StringAssert.Contains(diagnostics[0].GetMessage(), "'Use'");
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
    [DataRow("mocker.With<IOptions<Settings>>();")]
    [DataRow("mocker.With<IOptions<Settings>, OptionsWrapper<Settings>>();")]
    [DataRow("mocker.With<OptionsWrapper<Settings>>();")]
    [DataRow("mocker.With(typeof(IOptions<Settings>));")]
    [DataRow("mocker.With(typeof(IOptions<Settings>), typeof(OptionsWrapper<Settings>));")]
    [DataRow("mocker.With(typeof(OptionsWrapper<Settings>));")]
    [DataRow("mocker.With(implementationType: typeof(OptionsWrapper<Settings>), serviceType: typeof(IOptions<Settings>));")]
    public async Task WithOptionsRegistration_Reports(string body)
    {
        var diagnostics = await RunAsync(Wrap(body));
        Assert.HasCount(1, diagnostics);
        Assert.AreEqual(Diagnostics.PreferWithOptions.DiagnosticId, diagnostics[0].Id);
        StringAssert.Contains(diagnostics[0].GetMessage(), "WithOptions<Settings>()");
        StringAssert.Contains(diagnostics[0].GetMessage(), "'With'");
        StringAssert.StartsWith(diagnostics[0].Location.SourceTree!.GetText().ToString(diagnostics[0].Location.SourceSpan), "With");
    }

    [TestMethod]
    [DataRow("mocker.Use(typeof(IOptions<Settings>), options);")]
    [DataRow("mocker.Use(service: options, type: typeof(IOptions<Settings>));")]
    public async Task UseWithOptionsTypeAndObjectValue_Reports(string body)
    {
        var diagnostics = await RunAsync(Wrap("object options = Options.Create(new Settings()); " + body));
        Assert.HasCount(1, diagnostics);
        StringAssert.Contains(diagnostics[0].GetMessage(), "WithOptions<Settings>()");
    }

    [TestMethod]
    [DataRow("mocker.With<Settings>();")]
    [DataRow("mocker.With(typeof(Settings));")]
    [DataRow("mocker.Use<System.Type>(typeof(IOptions<Settings>));")]
    [DataRow("mocker.Use(typeof(System.Type), typeof(IOptions<Settings>));")]
    public async Task NonOptionsRegistration_DoesNotReport(string body)
    {
        var diagnostics = await RunAsync(Wrap(body));
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    [DataRow("new Other().Use(Options.Create(new Settings()));")]
    [DataRow("new Other().With<IOptions<Settings>>();")]
    public async Task UnrelatedMethod_DoesNotReport(string body)
    {
        var diagnostics = await RunAsync(Wrap(body) + """

            public class Other
            {
                public void Use<T>(T value) { }
                public void With<T>() { }
            }
            """);
        Assert.IsEmpty(diagnostics);
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
    [DataRow("mocker.Use(Options.Create(new Settings()));")]
    [DataRow("mocker.With<IOptions<Settings>, OptionsWrapper<Settings>>();")]
    [DataRow("mocker.With(typeof(IOptions<Settings>), typeof(OptionsWrapper<Settings>));")]
    public async Task OptionsRegistration_WhenExtensionNotGenerated_DoesNotReport(string body)
    {
        var diagnostics = await RunAsync(Wrap(body), runOptionsGenerator: false);
        Assert.IsEmpty(diagnostics);
    }
}
