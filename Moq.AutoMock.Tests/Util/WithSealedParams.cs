using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class WithSealedParams
{
    public string Sealed { get; set; }

    public WithSealedParams(string @sealed)
    {
        Sealed = @sealed;
    }
}
