﻿using System.Text;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VerifyCS = Moq.AutoMocker.Generators.Tests.CSharpSourceGeneratorVerifier<Moq.AutoMocker.Generators.UnitTestSourceGenerator>;

namespace Moq.AutoMocker.Generators.Tests;

[TestClass]
public class GeneratorsTests
{
    [TestMethod]
    public async Task Generation_WithProjectThatDoesNotReferenceAutoMocker_ProducesDiagnosticWarning()
    {
        var expectedResult =
            DiagnosticResult.CompilerWarning(Diagnostics.MustReferenceAutoMock.DiagnosticId);
        await new VerifyCS.Test
        {
            ReferenceAutoMocker = false,
            ExpectedDiagnostics =
            {
                expectedResult
            }
        }.RunAsync();
    }

    [TestMethod]
    public async Task Generation_WithDecoratedNonPartialClass_ProducesDiagnosticError()
    {
        var code = """

            using Moq.AutoMock;

            namespace TestNamespace;

            [ConstructorTests(TargetType = typeof(Controller))]
            public class ControllerTests
            {
                
            }

            public class Controller { }

            """;
        var expectedResult =
            DiagnosticResult.CompilerError(Diagnostics.TestClassesMustBePartial.DiagnosticId)
                        .WithSpan(6, 1, 10, 2)
                        .WithArguments("TestNamespace.ControllerTests");
        await new VerifyCS.Test
        {
            TestCode = code,
            ExpectedDiagnostics =
            {
                expectedResult
            }
        }.RunAsync();
    }

    [TestMethod]
    public async Task Generation_WithNoTargetTypeSpecified_ProducesDiagnosticError()
    {
        var code = """

            using Moq.AutoMock;

            namespace TestNamespace;

            [ConstructorTests]
            public class ControllerTests
            {
                
            }

            public class Controller { }

            """;
        var expectedResult =
            DiagnosticResult.CompilerError(Diagnostics.MustSpecifyTargetType.DiagnosticId)
                        .WithSpan(6, 2, 6, 18)
                        .WithArguments("TestNamespace.ControllerTests");
        await new VerifyCS.Test
        {
            TestCode = code,
            ExpectedDiagnostics =
            {
                expectedResult
            }
        }.RunAsync();
    }

    [TestMethod]
    [Description("Issue 142")]
    public async Task Generation_WithGenericParameter_RemovesInvalidCharactersFromTestsName()
    {
        var code = """
            using Moq.AutoMock;

            namespace TestNamespace;

            [ConstructorTests(typeof(Controller))]
            public partial class ControllerTests
            {
    
            }

            public class Controller
            {
                public Controller(ILogger<Controller> logger) { }
            }

            public interface ILogger<Controller> { }
            """;
        string expected = """
            namespace TestNamespace
            {
                partial class ControllerTests
                {
                    partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName);

                    partial void ControllerConstructor_WithNullILoggerController_ThrowsArgumentNullExceptionSetup(Moq.AutoMock.AutoMocker mocker);

                    public void ControllerConstructor_WithNullILoggerController_ThrowsArgumentNullException()
                    {
                        Moq.AutoMock.AutoMocker mocker = new Moq.AutoMock.AutoMocker();
                        AutoMockerTestSetup(mocker, "ControllerConstructor_WithNullILoggerController_ThrowsArgumentNullException");
                        ControllerConstructor_WithNullILoggerController_ThrowsArgumentNullExceptionSetup(mocker);
                        using(System.IDisposable __mockerDisposable = mocker.AsDisposable())
                        {
                        }
                    }

                }
            }

            """;

        await new VerifyCS.Test
        {
            TestCode = code,
            TestState =
            {
                GeneratedSources =
                {
                    GetSourceFile(expected, "ControllerTests.g.cs")
                }
            }
            
        }.RunAsync();
    }

    [TestMethod]
    public async Task Generation_WithValueTypeParameter_DoesNotGenerateTest()
    {
        var code = """
            using Moq.AutoMock;
            using System.Threading;

            namespace TestNamespace;

            [ConstructorTests(typeof(Controller))]
            public partial class ControllerTests
            {
                
            }

            public class Controller
            {
                public Controller(CancellationToken token) { }
            }

            """;
        string expected = """
            namespace TestNamespace
            {
                partial class ControllerTests
                {
                    partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName);

                }
            }

            """;

        await new VerifyCS.Test
        {
            TestCode = code,
            TestState =
            {
                GeneratedSources =
                {
                    GetSourceFile(expected, "ControllerTests.g.cs")
                }
            }

        }.RunAsync();
    }

    private static (string FileName, SourceText SourceText) GetSourceFile(string content, string fileName)
    {
        return (Path.Combine("Moq.AutoMocker.Generators", "Moq.AutoMocker.Generators.UnitTestSourceGenerator", fileName), SourceText.From(content, Encoding.UTF8));
    }
}
