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
    /// Skip generating tests for parameters that meet one of the following criteria:
    /// <list type="bullet">
    /// <item><description>Nullable reference types are enabled and the parameter type is a nullable reference type.</description></item>
    /// <item><description>The parameter is a nullable value type.</description></item>
    /// <item><description>The parameter has a default value of null.</description></item>
    /// </list>
    /// </summary>
    IgnoreNullableParameters
}
