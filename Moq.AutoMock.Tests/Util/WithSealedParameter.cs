namespace Moq.AutoMock.Tests
{
#pragma warning disable CA1801, CA1812  //is an internal class that is apparently never instantiated
    internal class WithSealedParameter
    {
        public WithSealedParameter() { }
        public WithSealedParameter(string @sealed) { }
    }
}
