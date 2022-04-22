namespace Moq.AutoMock;

/// <summary>
/// An attribute used by Moq.AutoMock.TestGenerator to generate unit tests for null constructor arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ConstructorTestsAttribute : Attribute
{
    /// <summary>
    /// The type of service to generate tests for.
    /// </summary>
    public Type? TargetType { get; set; }
    /// <summary>
    /// Create a new instance of the ConstructorTestsAttribute
    /// </summary>
    public ConstructorTestsAttribute()
    { }
    /// <summary>
    /// Create a new instance of the ConstructorTestsAttribute specifying the targetType
    /// </summary>
    /// <param name="targetType"></param>
    public ConstructorTestsAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}
