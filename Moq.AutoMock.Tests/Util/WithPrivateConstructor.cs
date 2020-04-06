namespace Moq.AutoMock.Tests.Util
{
#pragma warning disable CA1801, CA1812  //is an internal class that is apparently never instantiated
    internal class WithPrivateConstructor
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Local
        // ReSharper disable UnusedMember.Local
        public WithPrivateConstructor(IService1 service1) { }
        private WithPrivateConstructor(IService1 service1, IService2 service2) { }
        // ReSharper restore UnusedMember.Local
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Global
    }
}
