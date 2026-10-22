using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMocker.Generators.Tests;

[TestClass]
public class RedundantUseAnalyzerTests
{
    private static async Task<ImmutableArray<Diagnostic>> RunAsync(string body)
    {
        string source = $$"""
            using System;
            using Moq;
            using Moq.AutoMock;
            using MockAlias = Moq.Mock<IService>;

            public interface IService { int GetValue(); }
            public class Service : IService
            {
                public Service() { }
                public Service(int value) { }
                public virtual int GetValue() => 0;
            }
            public abstract class AbstractService { public abstract int GetValue(); }
            public class Lookalike
            {
                public void Use(object value) { }
                public Mock<IService> GetMock<T>() => new Mock<IService>();
            }
            public class Tests
            {
                private readonly AutoMocker _mocker = new AutoMocker();
                private AutoMocker Mocker => new AutoMocker();
                private AutoMocker GetMocker() => new AutoMocker();
                private void Configure(object value) { }
                public void Run(AutoMocker parameter)
                {
                    var mocker = new AutoMocker();
                    var other = new AutoMocker();
                    {{body}}
                }
            }
            """;
        string trusted = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        var references = trusted.Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
        Compilation compilation = CSharpCompilation.Create("Test",
            [CSharpSyntaxTree.ParseText(source)], references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Assert.IsEmpty(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray());

        var diagnostics = await compilation.WithAnalyzers([new RedundantUseAnalyzer()]).GetAnalyzerDiagnosticsAsync();
        Assert.IsTrue(diagnostics.All(d => d.Id == Diagnostics.RedundantUse.DiagnosticId),
            string.Join(Environment.NewLine, diagnostics));
        return diagnostics;
    }

    [TestMethod]
    [DataRow("mocker.Use(new Mock<IService>());")]
    [DataRow("mocker.Use(new Mock<IService>().Object);")]
    [DataRow("mocker.Use<IService>(new Mock<IService>().Object);")]
    [DataRow("mocker.Use(typeof(IService), new Mock<IService>().Object);")]
    [DataRow("mocker.Use(service: new Mock<IService>().Object, type: typeof(IService));")]
    [DataRow("mocker.Use(Mock.Of<IService>());")]
    [DataRow("mocker.Use(new MockAlias());")]
    [DataRow("Mock<IService> mock = new(); mocker.Use(mock);")]
    [DataRow("var mock = new Mock<IService>(); mocker.Use(mock.Object);")]
    [DataRow("var service = new Mock<IService>().Object; mocker.Use(service);")]
    [DataRow("var service = Mock.Of<IService>(); mocker.Use(service);")]
    public async Task UnconfiguredMock_Reports(string body)
    {
        var diagnostics = await RunAsync(body);
        Assert.HasCount(1, diagnostics);
        Assert.AreEqual(DiagnosticSeverity.Info, diagnostics[0].Severity);
        StringAssert.Contains(diagnostics[0].GetMessage(), "WithMock<T>()");
        Assert.AreEqual(body.Contains("Use<IService>", StringComparison.Ordinal) ? "Use<IService>" : "Use",
            diagnostics[0].Location.SourceTree!.GetText().ToString(diagnostics[0].Location.SourceSpan));
    }

    [TestMethod]
    [DataRow("mocker.Use(mocker.GetMock<IService>());")]
    [DataRow("mocker.Use(mocker.GetMock<IService>().Object);")]
    [DataRow("mocker.Use(mocker.GetMock<IService>(true));")]
    [DataRow("mocker.Use(typeof(IService), mocker.GetMock(typeof(IService)).Object);")]
    [DataRow("mocker.Use(service: mocker.GetMock(typeof(IService), true).Object, type: typeof(IService));")]
    [DataRow("var mock = mocker.GetMock<IService>(); mocker.Use(mock);")]
    [DataRow("var mock = mocker.GetMock<IService>(); mock.Setup(x => x.GetValue()).Returns(1); mocker.Use(mock.Object);")]
    [DataRow("var service = mocker.GetMock<IService>().Object; mocker.Use(service);")]
    [DataRow("_mocker.Use(this._mocker.GetMock<IService>());")]
    [DataRow("var mock = _mocker.GetMock<IService>(); _mocker.Use(mock);")]
    [DataRow("parameter.Use(parameter.GetMock<IService>());")]
    [DataRow("mocker.Use(((mocker.GetMock<IService>())).Object);")]
    [DataRow("var same = mocker.Use(mocker.GetMock<IService>());")]
    [DataRow("mocker.Use(mocker.GetMock<IService>()).GetMock<IDisposable>();")]
    public async Task RetainedMock_Reports(string body)
    {
        var diagnostics = await RunAsync(body);
        Assert.HasCount(1, diagnostics);
        StringAssert.Contains(diagnostics[0].GetMessage(), "already retains");
        Assert.IsFalse(diagnostics[0].GetMessage().Contains("WithMock", StringComparison.Ordinal));
    }

    [TestMethod]
    [DataRow("mocker.Use(new Mock<Service>());")]
    [DataRow("mocker.Use(new Mock<Service>().Object);")]
    [DataRow("mocker.Use<Service>(new Mock<Service>().Object);")]
    [DataRow("mocker.Use(typeof(Service), new Mock<Service>().Object);")]
    [DataRow("mocker.Use(Mock.Of<Service>());")]
    [DataRow("var mock = new Mock<Service>(); mocker.Use(mock);")]
    [DataRow("var service = Mock.Of<Service>(); mocker.Use(service);")]
    [DataRow("mocker.Use(new Mock<AbstractService>());")]
    [DataRow("mocker.Use(mocker.GetMock<Service>());")]
    [DataRow("mocker.Use(mocker.GetMock<Service>().Object);")]
    [DataRow("mocker.Use(typeof(Service), mocker.GetMock(typeof(Service)).Object);")]
    [DataRow("var mock = mocker.GetMock<Service>(); mocker.Use(mock);")]
    [DataRow("mocker.Use(mocker.GetMock<AbstractService>());")]
    public async Task ClassTarget_DoesNotReport(string body)
    {
        Assert.IsEmpty(await RunAsync(body));
    }

    [TestMethod]
    [DataRow("mocker.Use(new Mock<IService>(MockBehavior.Strict));")]
    [DataRow("mocker.Use(new Mock<Service>(42));")]
    [DataRow("mocker.Use(new Mock<IService> { CallBase = true });")]
    [DataRow("mocker.Use(Mock.Of<IService>(x => x.GetValue() == 1));")]
    [DataRow("var mock = new Mock<IService>(); mock.Setup(x => x.GetValue()).Returns(1); mocker.Use(mock);")]
    [DataRow("var mock = new Mock<IService>(); mocker.Use(mock); mock.Setup(x => x.GetValue()).Returns(1);")]
    [DataRow("var mock = new Mock<IService>(); mocker.Use(mock); mock.VerifyAll();")]
    [DataRow("var mock = new Mock<IService>(); Configure(mock); mocker.Use(mock);")]
    [DataRow("var mock = new Mock<IService>(); var alias = mock; mocker.Use(mock);")]
    [DataRow("var mock = mocker.GetMock<IService>(); mock = new Mock<IService>(); mocker.Use(mock);")]
    [DataRow("mocker.Use(other.GetMock<IService>());")]
    [DataRow("var mock = other.GetMock<IService>(); mocker.Use(mock);")]
    [DataRow("var mock = mocker.GetMock<IService>(); mocker = other; mocker.Use(mock);")]
    [DataRow("var mock = mocker.GetMock<IService>(); mocker.Use(new Mock<IService>(MockBehavior.Strict)); mocker.Use(mock);")]
    [DataRow("mocker.Use<object>(mocker.GetMock<IService>().Object);")]
    [DataRow("mocker.Use(typeof(object), mocker.GetMock(typeof(IService)).Object);")]
    [DataRow("mocker.Use<object>(new Mock<IService>().Object);")]
    [DataRow("mocker.Use(new Service(1));")]
    [DataRow("mocker.WithMock<IService>();")]
    [DataRow("mocker.WithMock(typeof(IService));")]
    [DataRow("mocker.Use<IService>(x => x.GetValue() == 1);")]
    [DataRow("mocker.Use<IService>(() => new Service(1));")]
    [DataRow("new Lookalike().Use(new Mock<IService>());")]
    [DataRow("mocker.Use(new Lookalike().GetMock<IService>());")]
    [DataRow("Mocker.Use(Mocker.GetMock<IService>());")]
    [DataRow("GetMocker().Use(GetMocker().GetMock<IService>());")]
    [DataRow("Mock<IService> mock = null!; mocker.Use(mock);")]
    [DataRow("var mock = new Mock<IService>(); Action configure = () => mock.CallBase = true; mocker.Use(mock);")]
    public async Task IntentionalOrUnknownRegistration_DoesNotReport(string body)
    {
        Assert.IsEmpty(await RunAsync(body));
    }
}
