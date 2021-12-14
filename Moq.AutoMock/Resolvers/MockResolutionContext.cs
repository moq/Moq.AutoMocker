using System;

namespace Moq.AutoMock.Resolvers;

/// <summary>
/// The context used to resolve types from an <c>AutoMocker</c> instance.
/// </summary>
public class MockResolutionContext
{
    private object? _value;

    /// <summary>
    /// Initializes an instance of MockResolutionContext.
    /// </summary>
    /// <param name="autoMocker">The <c>AutoMocker</c> instance.</param>
    /// <param name="requestType">The requested type to resolve.</param>
    /// <param name="objectGraphContext">
    /// Context within the object graph being created. This differs from the MockResolutionContext which is
    /// only relevant for a single object creation.
    /// </param>
    public MockResolutionContext(AutoMocker autoMocker, Type requestType, ObjectGraphContext objectGraphContext)
    {
        AutoMocker = autoMocker ?? throw new ArgumentNullException(nameof(autoMocker));
        RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
        ObjectGraphContext = objectGraphContext ?? throw new ArgumentNullException(nameof(objectGraphContext));
    }

    /// <summary>
    /// The <c>AutoMocker</c> instance.
    /// </summary>
    public AutoMocker AutoMocker { get; }

    /// <summary>
    /// The requested type to resolve.
    /// </summary>
    public Type RequestType { get; }

    /// <summary>
    /// The value to use from the resolution.
    /// </summary>
    public object? Value
    {
        get => _value;
        set
        {
            _value = value;
            ValueProvided = true;
        }
    }

    /// <summary>
    /// Context within the object graph being created. This differs from the MockResolutionContext which is
    /// only relevant for a single object creation.
    /// </summary>
    public ObjectGraphContext ObjectGraphContext { get; }

    /// <summary>
    /// Indicates if a value was set on the Value property.
    /// </summary>
    internal bool ValueProvided { get; private set; }

    /// <summary>
    /// Deconstruct this instance into its individual properties.
    /// </summary>
    /// <param name="autoMocker">The <c>AutoMocker</c> instance.</param>
    /// <param name="type">The requested type to resolve.</param>
    /// <param name="value">The value to use for the requested type.</param>
    public void Deconstruct(out AutoMocker autoMocker, out Type type, out object? value)
    {
        autoMocker = AutoMocker;
        type = RequestType;
        value = Value;
    }
}
