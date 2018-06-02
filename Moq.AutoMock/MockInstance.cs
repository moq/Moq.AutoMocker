namespace Moq.AutoMock
{
    internal sealed class MockInstance : IInstance
    {
        public MockInstance(Mock value) => Mock = value;

        public object Value => Mock.Object;
        public Mock Mock { get; }
        public bool IsMock => true;
    }
}
