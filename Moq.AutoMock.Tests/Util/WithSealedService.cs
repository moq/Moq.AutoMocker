using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class WithSealedService
{
    public SealedService SealedService { get; set; }

    public WithSealedService(SealedService service)
    {
        SealedService = service;
    }
}
