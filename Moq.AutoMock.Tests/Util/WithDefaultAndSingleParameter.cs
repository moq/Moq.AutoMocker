using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class WithDefaultAndSingleParameter
{
    public WithDefaultAndSingleParameter() { }
    public WithDefaultAndSingleParameter(IService1 _) { }
}
