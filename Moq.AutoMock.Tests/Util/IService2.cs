namespace Moq.AutoMock.Tests
{
    public interface IService2
    {
        IService1 Other { get; }
        string Name { get; }
    }
}
