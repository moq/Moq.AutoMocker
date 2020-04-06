using System;

namespace Moq.AutoMock.Resolvers
{
    /// <summary>
    /// The context used to resolve types from an <c>AutoMocker</c> instance.
    /// </summary>
    public class MockResolutionContext
    {
        /// <summary>
        /// Initializes an instance of MockResolutionContext.
        /// </summary>
        /// <param name="autoMocker">The <c>AutoMocker</c> instance.</param>
        /// <param name="requestType">The requested type to resolve.</param>
        /// <param name="initialValue">The initial value to use.</param>
        public MockResolutionContext(AutoMocker autoMocker, Type requestType, object? initialValue)
        {
            AutoMocker = autoMocker ?? throw new ArgumentNullException(nameof(autoMocker));
            RequestType = requestType ?? throw new ArgumentNullException(nameof(requestType));
            Value = initialValue;
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
        public object? Value { get; set; }

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
}
