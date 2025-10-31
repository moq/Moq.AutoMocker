using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Options;

namespace Moq.AutoMocker.Generators.Tests;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : ISourceGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
    {
        public bool ReferenceAutoMocker { get; set; } = true;
        public bool ReferenceOptionsAbstractions { get; set; }

        protected override Project ApplyCompilationOptions(Project project)
        {
            //project.AnalyzerOptions.WithAdditionalFiles();
            if (ReferenceAutoMocker || ReferenceOptionsAbstractions)
            {
                string fullPath = Path.GetFullPath($"{AutoMock.AssemblyName}.dll");
                project = project.AddMetadataReference(MetadataReference.CreateFromFile(fullPath));
            }

            if (ReferenceOptionsAbstractions)
            {
                project = project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(IOptions<>).Assembly.Location));
            }

            return base.ApplyCompilationOptions(project);
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
            string[] args = ["/warnaserror:nullable"];
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
