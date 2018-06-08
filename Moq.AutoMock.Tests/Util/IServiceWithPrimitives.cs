namespace Moq.AutoMock.Tests
{

    public interface IServiceWithPrimitives
    {
        long ReturnsALong();
        long ReturnsALongWithParameter(string parameter);
        string ReturnsAReferenceWithParameter(string parameter);
    }
}
