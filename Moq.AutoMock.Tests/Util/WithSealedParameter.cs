using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util
{
    [ExcludeFromCodeCoverage]
    public class WithSealedParameter
    {
        public WithSealedParameter() { }
        public WithSealedParameter(string _) { }
    }

    [ExcludeFromCodeCoverage]
    public class WithSealedParameter2
    {
        public WithSealedParameter2(string _) { }
    }
}
