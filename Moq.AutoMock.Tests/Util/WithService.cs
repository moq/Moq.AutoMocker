namespace Moq.AutoMock.Tests.Util
{
    public class WithService
    {
        public IService2 Service { get; set; }

        public WithService(IService2 service)
        {
            Service = service;
        }
    }
}
