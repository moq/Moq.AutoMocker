using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;

using VerifyCS = Moq.AutoMocker.TestGenerator.Tests.CSharpSourceGeneratorVerifier<Moq.AutoMocker.TestGenerator.UnitTestSourceGenerator>;

namespace Moq.AutoMocker.TestGenerator.Tests;

[TestClass]
public class TestGeneratorTests
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
        var code = @"
using Moq.AutoMock;

namespace TestNamespace;

[ConstructorTests(TargetType = typeof(Controller))]
public class ControllerTests
{
    
}

public class Controller { }
";
        var expectedResult =
            DiagnosticResult.CompilerError(Diagnostics.TestClassesMustBePartial.DiagnosticId)
                        .WithSpan(6, 1, 10, 2)
                        .WithArguments("TestNamespace.ControllerTests");
        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
            },
            ExpectedDiagnostics =
            {
                expectedResult
            }
        }.RunAsync();
    }

    [TestMethod]
    public async Task Generation_WithNoTargetTypeSpecified_ProducesDiagnosticError()
    {
        var code = @"
using Moq.AutoMock;

namespace TestNamespace;

[ConstructorTests]
public class ControllerTests
{
    
}

public class Controller { }
";
        var expectedResult =
            DiagnosticResult.CompilerError(Diagnostics.MustSpecifyTargetType.DiagnosticId)
                        .WithSpan(6, 2, 6, 18)
                        .WithArguments("TestNamespace.ControllerTests");
        await new VerifyCS.Test
        {
            TestState =
            {
                Sources = { code },
            },
            ExpectedDiagnostics =
            {
                expectedResult
            }
        }.RunAsync();
    }
}
