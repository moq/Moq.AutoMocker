namespace Moq.AutoMock.Tests
{
#pragma warning disable CA1801, CA1812  //is an internal class that is apparently never instantiated
    internal class With3Parameters
    {
        public With3Parameters() { }
        public With3Parameters(IService1 service1) { }
        public With3Parameters(IService1 service1, IService2 service2) { }
    }
}
