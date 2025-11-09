using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Moq.AutoMocker.Generators;

internal static class TestNameBuilder
{
    public static IEnumerable<string> CreateTestName(GeneratorTargetClass testClass, NullConstructorParameterTest test)
    {
        int testNameIndex = 0;
        string baseName = $"{testClass.Sut!.Name}Constructor_WithNull{test.NullTypeName}_ThrowsArgumentNullException";
        
        // Sanitize the base name first
        string sanitizedBaseName = SanitizeIdentifier(baseName);
        
        for (string testName = sanitizedBaseName;
            ;
            testName = $"{sanitizedBaseName}{++testNameIndex}")
        {
            yield return testName;
        }
    }
    
    private static string SanitizeIdentifier(string name)
    {
        if (SyntaxFacts.IsValidIdentifier(name))
        {
            return name;
        }
        
        StringBuilder sb = new(name.Length);
        for (int i = 0; i < name.Length; i++)
        {
            if (sb.Length == 0)
            {
                if (SyntaxFacts.IsIdentifierStartCharacter(name[i]))
                {
                    sb.Append(name[i]);
                }
            }
            else
            {
                if (SyntaxFacts.IsIdentifierPartCharacter(name[i]))
                {
                    sb.Append(name[i]);
                }
            }
        }
        return sb.ToString();
    }
}
