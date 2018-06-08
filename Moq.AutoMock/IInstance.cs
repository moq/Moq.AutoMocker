namespace Moq.AutoMock
{
    internal interface IInstance
    {
        object Value { get; }
        bool IsMock { get; }
    }
}
