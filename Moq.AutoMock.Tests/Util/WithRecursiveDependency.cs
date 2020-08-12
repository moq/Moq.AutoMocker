namespace Moq.AutoMock.Tests.Util
{
    public class WithRecursiveDependency
    {
        public WithRecursiveDependency Child { get; }
        
        public WithRecursiveDependency(WithRecursiveDependency child)
        {
            Child = child;
        }
    }
}