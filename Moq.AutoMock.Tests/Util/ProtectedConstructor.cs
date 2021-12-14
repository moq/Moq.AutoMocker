using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public abstract class ProtectedConstructor
{
    protected ProtectedConstructor() { }
}

[ExcludeFromCodeCoverage]
public abstract class ProtectedConstructor1
{
    protected ProtectedConstructor1(IService1 _) { }
}
