using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Threading.Tasks;
//using VerifyCS = Moq.AutoMocker.TestGenerator.Tests.CSharpCodeFixVerifier<
//    Analyzer1.Analyzer1Analyzer,
//    Analyzer1.Analyzer1CodeFixProvider>;

using VerifyCS = Moq.AutoMocker.TestGenerator.Tests.CSharpSourceGeneratorVerifier<Moq.AutoMocker.TestGenerator.UnitTestSourceGenerator>;


namespace Moq.AutoMocker.TestGenerator.Tests;



[TestClass]
public class UnitTest
{
    //No diagnostics expected to show up
    [TestMethod]
    public async Task TestMethod1()
    {
        var code = @"
namespace TestNamespace;

[ConstructorTests(TargetType = typeof(Controller))]
public partial class ControllerTests
{

}
";
        //var generated = "expected generated code";
        await new VerifyCS.Test
        {
            ReferenceAssemblies =
            {
                ReferenceAssemblyPath = ""
            },
            TestState =
            {
                Sources = { code },
            },
            ExpectedDiagnostics =
            {

            }
        }.RunAsync();

        //await VerifyCS.VerifyAnalyzerAsync(test);
    }
}
