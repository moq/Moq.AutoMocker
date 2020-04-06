namespace Moq.AutoMock.Tests.Util
{
    public class WithServiceInternal
    {
        public IService1? Service { get; set; }

        internal WithServiceInternal(IService1? service)
        {
            Service = service;
        }

        public WithServiceInternal() 
            : this(null)
        {
        }
    }
}
