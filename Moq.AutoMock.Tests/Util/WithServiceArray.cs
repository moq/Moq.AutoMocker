namespace Moq.AutoMock.Tests
{
    public class WithServiceArray
    {
        public IService2[] Services { get; set; }

        public WithServiceArray(IService2[] services)
        {
            Services = services;
        }
    }
}
