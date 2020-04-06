namespace Moq.AutoMock.Tests.Util
{
#pragma warning disable CA1801, CA1812  //is an internal class that is apparently never instantiated
    internal class WithDefaultAndSingleParameter
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Local
        public WithDefaultAndSingleParameter() { }
        public WithDefaultAndSingleParameter(IService1 service1) { }
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Global
    }
}
