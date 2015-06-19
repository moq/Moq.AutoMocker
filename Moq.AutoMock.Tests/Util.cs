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

    public interface IService3
    {
        string MainMethodName { get; }
    }

    public interface IService4
    {
        string MainMethodName(string input);

    }

    public interface IService5
    {
        string Name { get; set; }
    }

    public interface IServiceWithPrimitives
    {
        long ReturnsALong();
        long ReturnsALongWithParameter(string parameter);
        string ReturnsAReferenceWithParameter(string parameter);
    }
}
