using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util
{
    [ExcludeFromCodeCoverage]
    public class WithService
    {
        public IService2 Service { get; set; }

        public WithService(IService2 service)
        {
            Service = service;
        }
    }
}
