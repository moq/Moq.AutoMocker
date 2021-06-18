namespace Moq.AutoMock.Tests.Util
{
    public class HasClassDependency
    {
        public WithService WithService { get; }
        
        public HasClassDependency(WithService withService)
        {
            WithService = withService;
        }
    }
}