using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock
{
    internal sealed class MockInstance : IInstance
    {
        public MockInstance(Mock value) => Mock = value;

#if !NETSTANDARD2_0
        [NotNull]
#endif
        public object? Value => Mock.Object;
        public Mock Mock { get; }
        public bool IsMock => true;
    }
}
