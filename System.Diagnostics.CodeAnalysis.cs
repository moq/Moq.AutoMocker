// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1801, CA1822

namespace System.Diagnostics.CodeAnalysis
{
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    internal sealed class AllowNullAttribute : Attribute
    {
    }
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    internal sealed class DisallowNullAttribute : Attribute
    {
    }
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute
    {
    }
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        public DoesNotReturnIfAttribute(bool parameterValue) { }
        public bool ParameterValue { get { throw null!; } }
    }
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class MaybeNullAttribute : Attribute
    {
    }
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue) { }
        public bool ReturnValue { get { throw null!; } }
    }
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class NotNullAttribute : Attribute
    {
    }
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {
        public NotNullIfNotNullAttribute(string parameterName) { }
        public string ParameterName { get { throw null!; } }
    }
    [ExcludeFromCodeCoverage]
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue) { }
        public bool ReturnValue { get { throw null!; } }
    }
}