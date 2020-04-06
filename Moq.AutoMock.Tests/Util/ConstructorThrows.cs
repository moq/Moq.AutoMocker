using System;

namespace Moq.AutoMock.Tests
{
    public class ConstructorThrows
    {
        public ConstructorThrows()
        {
            throw new ArgumentException(string.Empty);
        }
    }
}
