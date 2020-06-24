namespace Moq.AutoMock.Tests.Util
{
#pragma warning disable CA1812  //is an internal class that is apparently never instantiated
    internal class With2ClassDependencies
#pragma warning restore CA1812
    {
        public WithService WithService { get; }
        public With3Parameters With3Parameters { get; }
        
        public With2ClassDependencies(WithService withService, With3Parameters with3Parameters)
        {
            WithService = withService;
            With3Parameters = with3Parameters;
        }
    }
}