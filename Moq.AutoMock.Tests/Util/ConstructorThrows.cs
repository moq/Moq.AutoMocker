using System;

namespace Moq.AutoMock.Tests.Util
{
    public class ConstructorThrows
    {
        public ConstructorThrows()
        {
            throw new ArgumentException(string.Empty);
        }
    }
}
