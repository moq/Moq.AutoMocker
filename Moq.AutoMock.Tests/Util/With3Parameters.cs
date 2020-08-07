namespace Moq.AutoMock.Tests.Util
{
    public class With3Parameters
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Local
        public With3Parameters() { }

#pragma warning disable CA1801  //Parameter  is never used. Remove the parameter or use it in the method body
        public With3Parameters(IService1 service1) { }
        public With3Parameters(IService1 service1, IService2 service2) { }
#pragma warning restore CA1801

        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Global
    }
}
