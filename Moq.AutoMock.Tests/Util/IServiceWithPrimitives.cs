namespace Moq.AutoMock.Tests.Util
{

    public interface IServiceWithPrimitives
    {
        long ReturnsALong();
        long ReturnsALongWithParameter(string parameter);
        string ReturnsAReferenceWithParameter(string parameter);
    }
}
