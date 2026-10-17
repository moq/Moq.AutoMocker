using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMocker.Generators.Tests;

[TestClass]
public class UseCreateInstanceAnalyzerTests
{
    private static Project CreateProject(string source)
    {
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var workspace = new AdhocWorkspace();
        Project project = workspace.AddProject("Test", LanguageNames.CSharp)
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable))
            .AddMetadataReferences(trusted.Split(Path.PathSeparator).Select(p => MetadataReference.CreateFromFile(p)));
        return project.AddDocument("Test.cs", SourceText.From(source)).Project;
    }

    private static async Task<ImmutableArray<Diagnostic>> RunAsync(Project project)
    {
        Compilation compilation = (await project.GetCompilationAsync())!;
        var diagnostics = await compilation.WithAnalyzers([new UseCreateInstanceAnalyzer()]).GetAnalyzerDiagnosticsAsync();
        return [.. diagnostics.Where(d => d.Id == Diagnostics.PreferWith.DiagnosticId)];
    }

    private static string Wrap(string body) => $$"""
        using System;
        using Moq.AutoMock;

        public interface IService { }
        public class Service : IService { }

        public class Tests
        {
            public void Run()
            {
                var mocker = new AutoMocker();
                var other = new AutoMocker();
                {{body}}
            }
        }
        """;

    private static async Task<string> FixAsync(string body)
    {
        Project project = CreateProject(Wrap(body));
        var diagnostics = await RunAsync(project);
        Assert.HasCount(1, diagnostics);

        Document document = project.Documents.Single();
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostics[0], (a, _) => actions.Add(a), CancellationToken.None);
        await new UseCreateInstanceCodeFixProvider().RegisterCodeFixesAsync(context);
        Assert.HasCount(1, actions);

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var applied = operations.OfType<ApplyChangesOperation>().Single();
        Document changed = applied.ChangedSolution.GetDocument(document.Id)!;
        string text = (await changed.GetTextAsync()).ToString();
        int start = text.IndexOf("var other = new AutoMocker();", StringComparison.Ordinal) + "var other = new AutoMocker();".Length;
        int end = text.LastIndexOf("}", text.LastIndexOf("}", StringComparison.Ordinal) - 1, StringComparison.Ordinal);
        return text[start..end].Trim().Replace("\r", "");
    }

    [TestMethod]
    public async Task UseCreateInstance_Reports()
    {
        var diagnostics = await RunAsync(CreateProject(Wrap("mocker.Use(mocker.CreateInstance<Service>());")));
        Assert.HasCount(1, diagnostics);
    }

    [TestMethod]
    public async Task UseExplicitServiceCreateInstance_Reports()
    {
        var diagnostics = await RunAsync(CreateProject(Wrap("mocker.Use<IService>(mocker.CreateInstance<Service>());")));
        Assert.HasCount(1, diagnostics);
    }

    [TestMethod]
    public async Task UseTypeCreateInstanceType_Reports()
    {
        var diagnostics = await RunAsync(CreateProject(Wrap("mocker.Use(typeof(IService), mocker.CreateInstance(typeof(Service)));")));
        Assert.HasCount(1, diagnostics);
    }

    [TestMethod]
    public async Task DifferentReceivers_DoesNotReport()
    {
        var diagnostics = await RunAsync(CreateProject(Wrap("mocker.Use(other.CreateInstance<Service>());")));
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task ResultUsed_DoesNotReport()
    {
        var diagnostics = await RunAsync(CreateProject(Wrap("var m = mocker.Use(mocker.CreateInstance<Service>());")));
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task CreateInstanceEnablePrivate_DoesNotReport()
    {
        var diagnostics = await RunAsync(CreateProject(Wrap("mocker.Use(mocker.CreateInstance<Service>(true));")));
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task UseWithNonCreateInstance_DoesNotReport()
    {
        var diagnostics = await RunAsync(CreateProject(Wrap("mocker.Use(new Service());")));
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task With_DoesNotReport()
    {
        var diagnostics = await RunAsync(CreateProject(Wrap("mocker.With<Service>();")));
        Assert.IsEmpty(diagnostics);
    }

    [TestMethod]
    public async Task Fix_InferredType() =>
        Assert.AreEqual("mocker.With<Service>();", await FixAsync("mocker.Use(mocker.CreateInstance<Service>());"));

    [TestMethod]
    public async Task Fix_SameExplicitType() =>
        Assert.AreEqual("mocker.With<Service>();", await FixAsync("mocker.Use<Service>(mocker.CreateInstance<Service>());"));

    [TestMethod]
    public async Task Fix_ServiceAndImplementation() =>
        Assert.AreEqual("mocker.With<IService, Service>();", await FixAsync("mocker.Use<IService>(mocker.CreateInstance<Service>());"));

    [TestMethod]
    public async Task Fix_Types() =>
        Assert.AreEqual("mocker.With(typeof(IService), typeof(Service));", await FixAsync("mocker.Use(typeof(IService), mocker.CreateInstance(typeof(Service)));"));
}
