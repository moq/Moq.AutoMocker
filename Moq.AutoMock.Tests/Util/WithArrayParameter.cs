namespace Moq.AutoMock.Tests.Util
{
    public class WithArrayParameter
    {
        public WithArrayParameter() { }

#pragma warning disable CA1801  //Parameter  is never used. Remove the parameter or use it in the method body
        public WithArrayParameter(string[] array) { }
        public WithArrayParameter(string[] array, string @sealed) { }
#pragma warning restore CA1801
    }
}
