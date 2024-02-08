using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace Moq.AutoMocker.Generators;

internal static class TestNameBuilder
{
    public static IEnumerable<string> CreateTestName(GeneratorTargetClass testClass, NullConstructorParameterTest test)
    {
        int testNameIndex = 0;
        for (string testName = $"{testClass.Sut!.Name}Constructor_WithNull{test.NullTypeName}_ThrowsArgumentNullException";
            ;
            testName = $"{testClass.Sut!.Name}Constructor_WithNull{test.NullTypeName}{++testNameIndex}_ThrowsArgumentNullException")
        {
            if (!SyntaxFacts.IsValidIdentifier(testName))
            {
                StringBuilder sb = new(testName.Length);
                for (int i = 0; i < testName.Length; i++)
                {
                    if (sb.Length == 0)
                    {
                        if (SyntaxFacts.IsIdentifierStartCharacter(testName[i]))
                        {
                            sb.Append(testName[i]);
                        }
                    }
                    else
                    {
                        if (SyntaxFacts.IsIdentifierPartCharacter(testName[i]))
                        {
                            sb.Append(testName[i]);
                        }
                    }
                }
                yield return sb.ToString();
            }
            else
            {
                yield return testName;
            }
        }
    }
}
