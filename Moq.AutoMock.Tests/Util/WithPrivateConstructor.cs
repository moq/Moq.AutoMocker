namespace Moq.AutoMock.Tests
{
#pragma warning disable CA1812  //is an internal class that is apparently never instantiated
    internal class WithPrivateConstructor
    {
        public WithPrivateConstructor(IService1 service1) { }
        private WithPrivateConstructor(IService1 service1, IService2 service2) { }
    }
}
