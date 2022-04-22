using Microsoft.CodeAnalysis;

namespace Moq.AutoMocker.TestGenerator;

public static class Diagnostics
{
    private const string Category = "AutoMocker.TestGenerator";
    public static class TestClassesMustBePartial
    {
        public const string DiagnosticId = "AMG0001";
        private const string Title = "Test class must be partial";
        private const string MessageFormat = "Class {0} is decorated with {1} attribute but is not declared as a partial class";
        private const string Description = "To generate constructor tests, the test class must be declared as partial. The tests are then generated into the partial class.";

        //NB: Do not make a property or use target-typed new expression
        //https://github.com/dotnet/roslyn-analyzers/issues/5890?msclkid=db74545bc13811ecac296aa1a3a09b53
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public static Diagnostic Create(Location? location, string className, string attributeName)
            => Diagnostic.Create(Rule, location, className, attributeName);
    }

    public static class MustReferenceAutoMock
    {
        public const string DiagnosticId = "AMG0002";
        private const string Title = $"Test projects must reference {AutoMock.AssemblyName}";
        private const string MessageFormat = $"To use Moq.AutoMocker.TestGenerator, your test project must also reference {AutoMock.AssemblyName}";
        private const string Description = $"Add a reference to the {AutoMock.AssemblyName} assembly or reference the {AutoMock.NuGetPackageName} NuGet package by running \"Install-Package Moq.AutoMock\".";

        //NB: Do not make a property or use target-typed new expression
        //https://github.com/dotnet/roslyn-analyzers/issues/5890?msclkid=db74545bc13811ecac296aa1a3a09b53
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public static Diagnostic Create()
            => Diagnostic.Create(Rule, null);
    }
}
