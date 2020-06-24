namespace Moq.AutoMock.Tests.Util
{
#pragma warning disable CA1801, CA1812  //is an internal class that is apparently never instantiated
    public class With3Parameters
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Local
        public With3Parameters() { }
        public With3Parameters(IService1 service1) { }
        public With3Parameters(IService1 service1, IService2 service2) { }
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Global
    }
}
