namespace Moq.AutoMock.Tests.Util
{
    public class WithPrivateConstructor
    {
        // ReSharper disable UnusedMember.Global
        // ReSharper disable UnusedParameter.Local
        // ReSharper disable UnusedMember.Local

#pragma warning disable CA1801  //Parameter  is never used. Remove the parameter or use it in the method body
        public WithPrivateConstructor(IService1 service1) { }
        private WithPrivateConstructor(IService1 service1, IService2 service2) { }
#pragma warning restore CA1801

        // ReSharper restore UnusedMember.Local
        // ReSharper restore UnusedParameter.Local
        // ReSharper restore UnusedMember.Global
    }
}
