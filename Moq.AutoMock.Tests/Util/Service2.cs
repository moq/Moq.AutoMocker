namespace Moq.AutoMock.Tests
{
    public class Service2 : IService2
    {
        public IService1 Other
        {
            get { return null; }
        }

        public string Name { get; set; }
    }
}
