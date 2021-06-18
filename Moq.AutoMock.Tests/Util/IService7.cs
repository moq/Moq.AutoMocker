namespace Moq.AutoMock.Tests.Util
{
    public interface IService7
    {
        void Void(int value);
        void Void(string value);

        object ReturnValue(int value);
        object ReturnValue(string value);
    }
}
