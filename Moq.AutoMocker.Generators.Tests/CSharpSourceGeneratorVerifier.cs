using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.Options;

namespace Moq.AutoMocker.Generators.Tests;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<EmptySourceGeneratorProvider, DefaultVerifier>
    {
        public bool ReferenceAutoMocker { get; set; } = true;
        public bool ReferenceOptionsAbstractions { get; set; }
        public bool ReferenceFakeLogging { get; set; }
        public bool ReferenceApplicationInsights { get; set; }
        public bool ReferenceDependencyInjection { get; set; }

        public void SetGlobalOption(string key, string value)
        {
            // Create or update .editorconfig content
            var existingConfig = TestState.AnalyzerConfigFiles.FirstOrDefault();
            var configBuilder = new StringBuilder();
            
            if (existingConfig.content != null)
            {
                configBuilder.Append(existingConfig.content);
            }
            else
            {
                configBuilder.AppendLine("is_global = true");
            }
            
            configBuilder.AppendLine($"{key} = {value}");
            
            TestState.AnalyzerConfigFiles.Clear();
            TestState.AnalyzerConfigFiles.Add(("/.globalconfig", configBuilder.ToString()));
        }

        protected override IEnumerable<Type> GetSourceGenerators()
        {
            yield return typeof(TSourceGenerator);
        }

        protected override Project ApplyCompilationOptions(Project project)
        {
            //project.AnalyzerOptions.WithAdditionalFiles();
            if (ReferenceAutoMocker || ReferenceOptionsAbstractions || ReferenceFakeLogging || ReferenceApplicationInsights || ReferenceDependencyInjection)
            {
                string fullPath = Path.GetFullPath($"{AutoMock.AssemblyName}.dll");
                project = project.AddMetadataReference(MetadataReference.CreateFromFile(fullPath));
            }

            if (ReferenceOptionsAbstractions)
            {
                project = project.AddMetadataReference(MetadataReference.CreateFromFile(typeof(IOptions<>).Assembly.Location));
            }

            if (ReferenceFakeLogging)
            {
                // Add reference to Microsoft.Extensions.Diagnostics.Testing and Microsoft.Extensions.Logging
                try
                {
                    // Try to load the assembly to get its location
                    var testingAssembly = typeof(Microsoft.Extensions.Logging.Testing.FakeLogger).Assembly;
                    project = project.AddMetadataReference(MetadataReference.CreateFromFile(testingAssembly.Location));
                    
                    var loggingAssembly = typeof(Microsoft.Extensions.Logging.ILogger).Assembly;
                    project = project.AddMetadataReference(MetadataReference.CreateFromFile(loggingAssembly.Location));
                }
                catch
                {
                    // If we can't find the assembly, the test will fail, which is appropriate
                }
            }

            if (ReferenceApplicationInsights)
            {
                // Add reference to Microsoft.ApplicationInsights
                try
                {
                    var appInsightsAssembly = typeof(Microsoft.ApplicationInsights.TelemetryClient).Assembly;
                    project = project.AddMetadataReference(MetadataReference.CreateFromFile(appInsightsAssembly.Location));
                }
                catch
                {
                    // If we can't find the assembly, the test will fail, which is appropriate
                }
            }

            if (ReferenceDependencyInjection)
            {
                // Add reference to Microsoft.Extensions.DependencyInjection.Abstractions
                try
                {
                    var diAssembly = typeof(Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider).Assembly;
                    project = project.AddMetadataReference(MetadataReference.CreateFromFile(diAssembly.Location));
                }
                catch
                {
                    // If we can't find the assembly, the test will fail, which is appropriate
                }
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

    internal class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private readonly TestAnalyzerConfigOptions _globalOptions;

        public TestAnalyzerConfigOptionsProvider(Dictionary<string, string> globalOptions)
        {
            _globalOptions = new TestAnalyzerConfigOptions(globalOptions);
        }

        public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => TestAnalyzerConfigOptions.Empty;

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => TestAnalyzerConfigOptions.Empty;
    }

    internal class TestAnalyzerConfigOptions : AnalyzerConfigOptions
    {
        private readonly Dictionary<string, string> _options;

        public static readonly TestAnalyzerConfigOptions Empty = new(new Dictionary<string, string>());

        public TestAnalyzerConfigOptions(Dictionary<string, string> options)
        {
            _options = options;
        }

        public override bool TryGetValue(string key, out string value)
        {
            return _options.TryGetValue(key, out value!);
        }
    }
}

