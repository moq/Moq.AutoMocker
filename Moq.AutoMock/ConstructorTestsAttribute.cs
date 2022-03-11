namespace Moq.AutoMock;

/// <summary>
/// TODO
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ConstructorTestsAttribute : Attribute
{
    /// <summary>
    /// TODO
    /// </summary>
    public Type? TargetType { get; set; }
    /// <summary>
    /// TODO
    /// </summary>
    public ConstructorTestsAttribute()
    { }
    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="targetType"></param>
    public ConstructorTestsAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}
