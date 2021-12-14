using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class WithRecursiveDependency
{
    public WithRecursiveDependency Child { get; }

    public WithRecursiveDependency(WithRecursiveDependency child)
    {
        Child = child;
    }
}
