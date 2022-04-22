using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Moq.AutoMocker.TestGenerator.Tests;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : ISourceGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, MSTestVerifier>
    {
        public Test()
        {
            //string fullPath = Path.GetFullPath("Moq.AutoMock.dll");
            //bool exists = File.Exists(fullPath);
            
            //ReferenceAssemblies = ReferenceAssemblies.AddAssemblies(ImmutableArray.Create("Moq.AutoMock"));
        }

        protected override Project ApplyCompilationOptions(Project project)
        {
            string fullPath = Path.GetFullPath("Moq.AutoMock.dll");
            project = project.AddMetadataReference(MetadataReference.CreateFromFile(fullPath));
            return base.ApplyCompilationOptions(project);
        }

        protected override CompilationWithAnalyzers CreateCompilationWithAnalyzers(Compilation compilation, ImmutableArray<DiagnosticAnalyzer> analyzers, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            //string fullPath = Path.GetFullPath("Moq.AutoMock.dll");
            //compilation = compilation.AddReferences(MetadataReference.CreateFromFile(fullPath));

            return base.CreateCompilationWithAnalyzers(compilation, analyzers, options, cancellationToken);
        }

        protected override CompilationOptions CreateCompilationOptions()
        {
            var compilationOptions = base.CreateCompilationOptions();
            return compilationOptions.WithSpecificDiagnosticOptions(
                 compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
            var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

            return nullableWarnings;
        }

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
        }
    }
}
