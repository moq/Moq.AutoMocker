namespace Moq.AutoMock.Tests
{
    public class OneConstructor
    {
        public Empty Empty;

        public OneConstructor(Empty empty)
        {
            Empty = empty;
        }
    }
}
