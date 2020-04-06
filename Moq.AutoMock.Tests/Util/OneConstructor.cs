namespace Moq.AutoMock.Tests
{
    public class OneConstructor
    {
        public Empty Empty { get; }

        public OneConstructor(Empty empty)
        {
            Empty = empty;
        }
    }
}
