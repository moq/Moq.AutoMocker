using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock;

[DebuggerDisplay("Mock: {Mock.Object.GetType().Name,nq}")]
internal sealed class MockInstance(Mock value) : IInstance
{
    [NotNull]
    public object? Value => Mock.Object;
    public Mock Mock { get; } = value;
    public bool IsMock => true;
}
