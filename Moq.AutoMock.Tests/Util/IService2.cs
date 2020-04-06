namespace Moq.AutoMock.Tests.Util
{
    public interface IService2
    {
        IService1 Other { get; }
        string Name { get; }
    }
}
