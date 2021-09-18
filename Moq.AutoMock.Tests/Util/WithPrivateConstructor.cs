using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util
{
    [ExcludeFromCodeCoverage]
    public class WithPrivateConstructor
    {
#pragma warning disable CA1801  //Parameter  is never used. Remove the parameter or use it in the method body
        public WithPrivateConstructor(IService1 service1) { }
        private WithPrivateConstructor(IService1 service1, IService2 service2) { }
#pragma warning restore CA1801
    }
}
