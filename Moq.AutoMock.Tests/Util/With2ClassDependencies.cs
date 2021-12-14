using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class With2ClassDependencies
{
    public WithService WithService { get; }
    public With3Parameters With3Parameters { get; }

    public With2ClassDependencies(WithService withService, With3Parameters with3Parameters)
    {
        WithService = withService;
        With3Parameters = with3Parameters;
    }
}
