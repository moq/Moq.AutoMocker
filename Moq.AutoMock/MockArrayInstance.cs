using System.Diagnostics;

namespace Moq.AutoMock;

[DebuggerDisplay("Array: {_type.Name,nq}[] (Count = {_mocks.Count})")]
internal sealed class MockArrayInstance(Type type) : IInstance
{
    private readonly Type _type = type;
    private readonly List<IInstance> _mocks = [];

    public object Value
    {
        get
        {
            int i = 0;
            Array array = Array.CreateInstance(_type, _mocks.Count);
            foreach (IInstance instance in _mocks)
                array.SetValue(instance.Value, i++);
            return array;
        }
    }

    public bool IsMock { get { return _mocks.Any(m => m.IsMock); } }

    public void Add(IInstance instance)
    {
        _mocks.Add(instance);
    }
}
