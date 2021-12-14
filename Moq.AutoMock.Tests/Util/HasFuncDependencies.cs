using System;
using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util;

[ExcludeFromCodeCoverage]
public class HasFuncDependencies
{
    public Func<WithService> WithServiceFactory { get; }

    public HasFuncDependencies(Func<WithService> withServiceFactory)
    {
        WithServiceFactory = withServiceFactory;
    }
}
