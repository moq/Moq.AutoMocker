using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Moq.AutoMock;

/// <summary>
/// This exception is thrown when the <see cref="AutoMocker"/> is unable to create an instance of a type.
/// </summary>
public class ObjectCreationException : Exception
{
    /// <summary>
    /// A list of diagnostic messages indicating why the object could not be created.
    /// </summary>
    public IReadOnlyList<string> DiagnosticMessages { get; } = Array.Empty<string>();

    /// <summary>
    /// Default constructor for <see cref="ObjectCreationException"/>.
    /// </summary>
    /// <param name="diagnosticMessages">A list of diagnostic messages</param>
    public ObjectCreationException(IReadOnlyList<string> diagnosticMessages)
    {
        DiagnosticMessages = diagnosticMessages ?? throw new ArgumentNullException(nameof(diagnosticMessages));
    }

    /// <summary>
    /// Constructor for <see cref="ObjectCreationException"/>.
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="diagnosticMessages">A list of diagnostic messages</param>
    public ObjectCreationException(string message, IReadOnlyList<string> diagnosticMessages) : base(message)
    {
        DiagnosticMessages = diagnosticMessages ?? throw new ArgumentNullException(nameof(diagnosticMessages));
    }

    /// <summary>
    /// Constructor for <see cref="ObjectCreationException"/>.
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="diagnosticMessages">A list of diagnostic messages</param>
    public ObjectCreationException(string message, Exception innerException, IReadOnlyList<string> diagnosticMessages) : base(message, innerException)
    {
        DiagnosticMessages = diagnosticMessages ?? throw new ArgumentNullException(nameof(diagnosticMessages));
    }

    /// <summary>
    /// Serialization constructor for <see cref="ObjectCreationException"/>.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected ObjectCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
