using Microsoft.CodeAnalysis;

namespace Moq.AutoMocker.TestGenerator;

public static class Diagnostics
{
    public static class TestClassesMustBePartial
    {
        public const string DiagnosticId = "AMG0001";
        private const string Title = "Test class must be partial";
        private const string MessageFormat = "Class {0} is decorated with {1} attribute but is not declared as a partial class";
        private const string Description = "To generate constructor tests, the test class must be declared as partial. The tests are then generated into the partial class.";
        private const string Category = "AutoMocker.TestGenerator";

        //NB: Do not make a property or use target-typed new expression
        //https://github.com/dotnet/roslyn-analyzers/issues/5890?msclkid=db74545bc13811ecac296aa1a3a09b53
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public static Diagnostic Create(Location? location, string className, string attributeName)
            => Diagnostic.Create(Rule, location, className, attributeName);
    }


}
