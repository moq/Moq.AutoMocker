using System;
using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util
{
    [ExcludeFromCodeCoverage]
    public class ConstructorThrows
    {
        public ConstructorThrows()
        {
            throw new ArgumentException(string.Empty);
        }
    }
}
