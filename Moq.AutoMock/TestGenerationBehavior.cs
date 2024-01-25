namespace Moq.AutoMock;

/// <summary>
/// Control the behavior of the test generator, such as whether to skip nullable parameters.
/// </summary>
public enum TestGenerationBehavior
{
    /// <summary>
    /// The default behavior, which is to generate tests for all parameters.
    /// </summary>
    Default,

    /// <summary>
    /// Skip parameters that meet one of the following criteria:
    /// - Nullable reference types are enabled and the parameter type is a nullable reference type
    /// - The parameter is a nullable value type
    /// - The parameter has a default value of null
    /// </summary>
    IgnoreNullableParameters
}
