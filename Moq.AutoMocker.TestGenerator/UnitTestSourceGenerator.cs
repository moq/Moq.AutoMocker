using System.Text;
using Microsoft.CodeAnalysis;

namespace Moq.AutoMocker.TestGenerator;

[Generator]
public class UnitTestSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        var testingFramework = GetTestingFramework(context.Compilation.ReferencedAssemblyNames);
        SyntaxReceiver rx = (SyntaxReceiver)context.SyntaxContextReceiver!;

        foreach (GeneratorTargetClass testClass in rx.TestClasses)
        {
            StringBuilder builder = new();

            builder.AppendLine($"namespace {testClass.Namespace}");
            builder.AppendLine("{");

            builder.AppendLine($"    partial class {testClass.TestClassName}");
            builder.AppendLine("    {");


            foreach (var test in testClass.Sut?.NullConstructorParameterTests ?? Enumerable.Empty<NullConstructorParameterTest>())
            {
                switch (testingFramework)
                {
                    case TargetTestingFramework.MSTest:
                        builder.AppendLine("        [global::Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]");
                        break;
                    case TargetTestingFramework.Xunit:
                        builder.AppendLine("        [global::Xunit.Fact]");
                        break;
                    case TargetTestingFramework.NUnit:
                        builder.AppendLine("        [global::NUnit.Framework.Test]");
                        break;
                }

                builder.AppendLine($"        public void {testClass.Sut!.Name}Constructor_WithNull{test.NullTypeName}_ThrowsArgumentNullException()");
                builder.AppendLine("        {");
                builder.AppendLine("            Moq.AutoMock.AutoMocker mocker = new Moq.AutoMock.AutoMocker();");
                builder.AppendLine($"            mocker.Use(typeof({test.NullTypeFullName}), null);");

                switch (testingFramework)
                {
                    case TargetTestingFramework.MSTest:
                        builder.AppendLine($"            System.ArgumentNullException ex = global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowsException<System.ArgumentNullException>(() => mocker.CreateInstance<{testClass.Sut.FullName}>());");
                        builder.AppendLine($"            global::Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(\"{test.ParameterName}\", ex.ParamName);");
                        break;
                    case TargetTestingFramework.Xunit:
                        builder.AppendLine($"            System.ArgumentNullException ex = global::Xunit.Assert.Throws<System.ArgumentNullException>(() => mocker.CreateInstance<{testClass.Sut.FullName}>());");
                        builder.AppendLine($"            global::Xunit.Assert.Equal(\"{test.ParameterName}\", ex.ParamName);");
                        break;
                    case TargetTestingFramework.NUnit:
                        builder.AppendLine($"            System.ArgumentNullException ex = global::NUnit.Framework.Assert.Throws<System.ArgumentNullException>(() => mocker.CreateInstance<{testClass.Sut.FullName}>());");
                        builder.AppendLine($"            global::NUnit.Framework.Assert.AreEqual(\"{test.ParameterName}\", ex.ParamName);");
                        break;
                }

                builder.AppendLine("        }");
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            context.AddSource($"{testClass.TestClassName}.g.cs", builder.ToString());

        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached)
        {
            //System.Diagnostics.Debugger.Launch();
        }
#endif
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static TargetTestingFramework GetTestingFramework(IEnumerable<AssemblyIdentity> assemblies)
    {
        foreach (AssemblyIdentity assembly in assemblies)
        {
            if (assembly.Name.StartsWith("Microsoft.VisualStudio.TestPlatform.TestFramework"))
            {
                return TargetTestingFramework.MSTest;
            }
            if (assembly.Name.StartsWith("nunit."))
            {
                return TargetTestingFramework.NUnit;
            }
            if (assembly.Name.StartsWith("xunit."))
            {
                return TargetTestingFramework.Xunit;
            }
        }
        return TargetTestingFramework.Unknown;
    }
}

public enum TargetTestingFramework
{
    Unknown,
    MSTest,
    Xunit,
    NUnit
}
