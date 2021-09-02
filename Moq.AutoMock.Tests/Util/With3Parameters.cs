using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util
{
    [ExcludeFromCodeCoverage]
    public class With3Parameters
    {
        public With3Parameters() { }

#pragma warning disable CA1801  //Parameter  is never used. Remove the parameter or use it in the method body
        public With3Parameters(IService1 _) { }
        public With3Parameters(IService1 service1, IService2 service2) { }
#pragma warning restore CA1801
    }
}
