namespace Moq.AutoMock.Tests
{
    public interface IService1 {}
    public interface IService2
    {
        IService1 Other { get; }
    }

    public class Service2 : IService2
    {
        public IService1 Other
        {
            get { return null; }
        }
    }
}
