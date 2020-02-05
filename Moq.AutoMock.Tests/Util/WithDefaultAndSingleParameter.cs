namespace Moq.AutoMock.Tests
{
#pragma warning disable CA1812  //is an internal class that is apparently never instantiated
    internal class WithDefaultAndSingleParameter
    {
        public WithDefaultAndSingleParameter() { }
        public WithDefaultAndSingleParameter(IService1 service1) { }
    }
}
