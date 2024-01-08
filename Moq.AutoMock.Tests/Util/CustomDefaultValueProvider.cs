namespace Moq.AutoMock.Tests.Util;
internal class CustomDefaultValueProvider : DefaultValueProvider
{
    public Dictionary<Type, object> DefaultValues { get; } = new();

    protected override object GetDefaultValue(Type type, Mock mock)
    {
        if (DefaultValues.TryGetValue(type, out object? value))
        {
            return value;
        }
        throw new NotImplementedException();
    }
}
