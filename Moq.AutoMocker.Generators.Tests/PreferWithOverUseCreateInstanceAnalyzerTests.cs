using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMocker.Generators.Tests;

[TestClass]
public class PreferWithOverUseCreateInstanceAnalyzerTests
{
    [TestMethod]
    public async Task ReportsDiagnosticForUseCreateInstancePattern()
    {
        const string source = """
            using Moq.AutoMock;

            public class TestClass
            {
                public void Test()
                {
                    var mocker = new AutoMocker();
                    mocker.Use(mocker.CreateInstance<IDisposable>());
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.AreEqual(1, diagnostics.Length);
        Assert.AreEqual(Diagnostics.PreferWithOverUseCreateInstance.DiagnosticId, diagnostics[0].Id);
    }

    [TestMethod]
    public async Task DoesNotReportDiagnosticForWithPattern()
    {
        const string source = """
            using Moq.AutoMock;

            public class TestClass
            {
                public void Test()
                {
                    var mocker = new AutoMocker();
                    mocker.With<IDisposable>();
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.AreEqual(0, diagnostics.Length);
    }

    [TestMethod]
    public async Task DoesNotReportDiagnosticWhenCreateInstanceUsesDifferentReceiver()
    {
        const string source = """
            using Moq.AutoMock;

            public class TestClass
            {
                public void Test()
                {
                    var mocker = new AutoMocker();
                    var other = new AutoMocker();
                    mocker.Use(other.CreateInstance<IDisposable>());
                }
            }
            """;

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.AreEqual(0, diagnostics.Length);
    }

    private static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(Moq.AutoMock.AutoMocker).Assembly.Location))
            .ToList();

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new PreferWithOverUseCreateInstanceAnalyzer();
        var diagnostics = await compilation
            .WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync();

        return diagnostics.Where(x => x.Id == Diagnostics.PreferWithOverUseCreateInstance.DiagnosticId).ToImmutableArray();
    }
}
