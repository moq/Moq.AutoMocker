using System.Linq;

namespace Moq.AutoMock
{

    internal sealed class RealInstance : IInstance
    {
        public RealInstance(object value) => Value = value;

        public object Value { get; }
        public bool IsMock => false;
    }
}
