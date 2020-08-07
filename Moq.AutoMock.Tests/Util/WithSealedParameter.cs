namespace Moq.AutoMock.Tests.Util
{
    public class WithSealedParameter
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Local
        public WithSealedParameter() { }

#pragma warning disable CA1801  //Parameter  is never used. Remove the parameter or use it in the method body
        public WithSealedParameter(string @sealed) { }
#pragma warning restore CA1801  //Parameter  is never used. Remove the parameter or use it in the method body

        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Global
    }
}
