using System.Diagnostics.CodeAnalysis;

namespace Moq.AutoMock.Tests
{
    public class WithServiceArray
    {
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The point is to test the array.")]
        public IService2[] Services { get; set; }

        public WithServiceArray(IService2[] services)
        {
            Services = services;
        }
    }
}
