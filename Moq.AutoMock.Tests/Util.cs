namespace Moq.AutoMock.Tests
{
    public interface IService1
    {
        void Void();
    }
    public interface IService2
    {
        IService1 Other { get; }
        string Name { get; }
    }

    public class Service2 : IService2
    {
        public IService1 Other
        {
            get { return null; }
        }

        public string Name { get; set; }
    }
}
