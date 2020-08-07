namespace Moq.AutoMock.Tests.Util
{
    public class WithDefaultAndSingleParameter
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Local
        public WithDefaultAndSingleParameter() { }

#pragma warning disable CA1801  //Parameter  is never used. Remove the parameter or use it in the method body
        public WithDefaultAndSingleParameter(IService1 service1) { }
#pragma warning restore CA1801

        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Global
    }
}
