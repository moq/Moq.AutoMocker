using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class HasClassDependency
{
    public WithService WithService { get; }

    public HasClassDependency(WithService withService)
    {
        WithService = withService;
    }
}
