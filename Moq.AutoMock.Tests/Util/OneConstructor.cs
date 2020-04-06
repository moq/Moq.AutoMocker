namespace Moq.AutoMock.Tests.Util
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
