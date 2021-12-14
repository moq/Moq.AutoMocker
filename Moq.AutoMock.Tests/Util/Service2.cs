using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class Service2 : IService2
{
    public IService1 Other
    {
        get { return null!; }
    }

    public string Name { get; set; } = string.Empty;
}
