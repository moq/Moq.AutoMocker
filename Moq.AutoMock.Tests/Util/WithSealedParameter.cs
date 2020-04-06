namespace Moq.AutoMock.Tests.Util
{
#pragma warning disable CA1801, CA1812  //is an internal class that is apparently never instantiated
    internal class WithSealedParameter
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Local
        public WithSealedParameter() { }
        public WithSealedParameter(string @sealed) { }
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Global
    }
}
