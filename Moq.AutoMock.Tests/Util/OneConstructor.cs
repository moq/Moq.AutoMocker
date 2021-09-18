using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests.Util
{
    [ExcludeFromCodeCoverage]
    public class OneConstructor
    {
        public Empty Empty { get; }

        public OneConstructor(Empty empty)
        {
            Empty = empty;
        }
    }
}
