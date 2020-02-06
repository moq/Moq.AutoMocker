// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable CA1801, CA1822

namespace System.Diagnostics.CodeAnalysis
{
    [System.AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, Inherited = false)]
    internal sealed partial class AllowNullAttribute : System.Attribute
    {
        public AllowNullAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property, Inherited = false)]
    internal sealed partial class DisallowNullAttribute : System.Attribute
    {
        public DisallowNullAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Method, Inherited = false)]
    internal sealed partial class DoesNotReturnAttribute : System.Attribute
    {
        public DoesNotReturnAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Parameter, Inherited = false)]
    internal sealed partial class DoesNotReturnIfAttribute : System.Attribute
    {
        public DoesNotReturnIfAttribute(bool parameterValue) { }
        public bool ParameterValue { get { throw null!; } }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Constructor | System.AttributeTargets.Event | System.AttributeTargets.Method | System.AttributeTargets.Property | System.AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    internal sealed partial class ExcludeFromCodeCoverageAttribute : System.Attribute
    {
        public ExcludeFromCodeCoverageAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed partial class MaybeNullAttribute : System.Attribute
    {
        public MaybeNullAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Parameter, Inherited = false)]
    internal sealed partial class MaybeNullWhenAttribute : System.Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue) { }
        public bool ReturnValue { get { throw null!; } }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Field | System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed partial class NotNullAttribute : System.Attribute
    {
        public NotNullAttribute() { }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Parameter | System.AttributeTargets.Property | System.AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    internal sealed partial class NotNullIfNotNullAttribute : System.Attribute
    {
        public NotNullIfNotNullAttribute(string parameterName) { }
        public string ParameterName { get { throw null!; } }
    }
    [System.AttributeUsageAttribute(System.AttributeTargets.Parameter, Inherited = false)]
    internal sealed partial class NotNullWhenAttribute : System.Attribute
    {
        public NotNullWhenAttribute(bool returnValue) { }
        public bool ReturnValue { get { throw null!; } }
    }
}