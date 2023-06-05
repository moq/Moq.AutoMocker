using System.Reflection;

namespace Moq.AutoMock;

/// <summary>
/// Handles state while creating an object graph.
/// </summary>
public class ObjectGraphContext
{
    private List<string>? _diagnostics;

    /// <summary>
    /// Creates an instance with binding flags set according to `enablePrivate`.
    /// </summary>
    /// <param name="enablePrivate"></param>
    public ObjectGraphContext(bool enablePrivate)
    {
        BindingFlags = GetBindingFlags(enablePrivate);
        VisitedTypes = new HashSet<Type>();
    }

    /// <summary>
    /// Creates a new instance, copying the values for the Binding flags and the Visited types.
    /// </summary>
    /// <param name="context"></param>
    public ObjectGraphContext(ObjectGraphContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        BindingFlags = context.BindingFlags;
        VisitedTypes = new HashSet<Type>(context.VisitedTypes);
    }

    /// <summary>
    /// Flags passed to Mock constructor.
    /// </summary>
    public BindingFlags BindingFlags { get; }

    /// <summary>
    /// Used internally to track which types have been created inside a call graph,
    /// to detect cycles in the object graph.
    /// </summary>
    public HashSet<Type> VisitedTypes { get; }

    /// <summary>
    /// A list of diagnostic messages.
    /// </summary>
    public IReadOnlyList<string> DiagnosticMessages => _diagnostics ?? (IReadOnlyList<string>)Array.Empty<string>();

    /// <summary>
    /// Add a new diagnostic message to this context.
    /// </summary>
    /// <param name="message">The message to be added</param>
    public void AddDiagnosticMessage(string message)
    {
        _diagnostics ??= new();
        _diagnostics.Add(message);
    }

    private static BindingFlags GetBindingFlags(bool enablePrivate)
    {
        var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
        if (enablePrivate) bindingFlags |= BindingFlags.NonPublic;
        return bindingFlags;
    }
}
