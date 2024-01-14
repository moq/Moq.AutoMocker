using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class PrimaryConstructor(IService1 service)
{
    public IService1 Service { get; set; } = service;
}