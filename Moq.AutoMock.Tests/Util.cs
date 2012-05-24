namespace Moq.AutoMock.Tests
{
    public interface IService1 {}
    public interface IService2
    {
        IService1 Other { get; }
    }
}
