using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
[SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "That's kind of the point here")]
public class WithStatic
{
    public static string Get()
    {
        return string.Empty;
    }
}
