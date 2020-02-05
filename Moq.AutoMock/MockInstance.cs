using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock
{
    internal sealed class MockInstance : IInstance
    {
        public MockInstance(Mock value) => Mock = value;

        [NotNull]
        public object? Value => Mock.Object;
        public Mock Mock { get; }
        public bool IsMock => true;
    }
}
