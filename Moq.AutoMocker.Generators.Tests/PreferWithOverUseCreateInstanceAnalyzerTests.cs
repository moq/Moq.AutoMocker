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
            using System;
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
            using System;
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
            using System;
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

        var trustedPlatformAssemblyPaths = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
            .Split(Path.PathSeparator);

        var loadedAssemblyPaths = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => assembly.Location);

        var references = trustedPlatformAssemblyPaths
            .Concat(loadedAssemblyPaths)
            .Append(typeof(Moq.AutoMock.AutoMocker).Assembly.Location)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(path => MetadataReference.CreateFromFile(path))
            .ToList();

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var compilationErrors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.AreEqual(0, compilationErrors.Length, string.Join(Environment.NewLine, compilationErrors.Select(x => x.ToString())));

        var analyzer = new PreferWithOverUseCreateInstanceAnalyzer();
        var diagnostics = await compilation
            .WithAnalyzers([analyzer])
            .GetAnalyzerDiagnosticsAsync();

        return diagnostics.Where(x => x.Id == Diagnostics.PreferWithOverUseCreateInstance.DiagnosticId).ToImmutableArray();
    }
}
