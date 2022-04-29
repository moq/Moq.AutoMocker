﻿namespace Moq.AutoMock.Resolvers;

/// <summary>
/// A resolver that resolves requested types with Mock&lt;T&gt; instances.
/// </summary>
public class MockResolver : IMockResolver
{
    private readonly MockBehavior _mockBehavior;
    private readonly DefaultValue _defaultValue;
    private readonly DefaultValueProvider? _defaultValueProvider;
    private readonly bool _callBase;

    /// <summary>
    /// Initializes an instance of <c>MockResolver</c>.
    /// </summary>
    /// <param name="mockBehavior">Behavior of created mock.</param>
    /// <param name="defaultValue">Specifies the behavior to use when returning default values for 
    /// unexpected invocations on loose mocks created by this instance.</param>
    /// <param name="callBase">Whether the base member virtual implementation will be called 
    /// for created mocks if no setup is matched.</param>
    public MockResolver(MockBehavior mockBehavior, DefaultValue defaultValue, bool callBase)
        : this(mockBehavior, defaultValue, null, callBase)
    { }

    /// <summary>
    /// Initializes an instance of <c>MockResolver</c>.
    /// </summary>
    /// <param name="mockBehavior">Behavior of created mock.</param>
    /// <param name="defaultValue">Specifies the behavior to use when returning default values for 
    /// unexpected invocations on loose mocks created by this instance.</param>
    /// <param name="defaultValueProvider">The instance that will be used to produce default return values for unexpected invocations.</param>
    /// <param name="callBase">Whether the base member virtual implementation will be called 
    /// for created mocks if no setup is matched.</param>
    public MockResolver(
        MockBehavior mockBehavior, 
        DefaultValue defaultValue, 
        DefaultValueProvider? defaultValueProvider, 
        bool callBase)
    {
        _mockBehavior = mockBehavior;
        _defaultValue = defaultValue;
        _defaultValueProvider = defaultValueProvider;
        _callBase = callBase;
    }

    /// <summary>
    /// Resolves requested types with Mock instances.
    /// </summary>
    /// <param name="context">The resolution context.</param>
    public void Resolve(MockResolutionContext context)
    {
        if (context.RequestType == typeof(string)) return;

        if (context.AutoMocker.CreateMock(
            context.RequestType,
            _mockBehavior,
            _defaultValue,
            _defaultValueProvider,
            _callBase, 
            context.ObjectGraphContext) is { } mock)
        {
            context.Value = mock;
        }
    }
}
