# Diagnostics and constructor tests

Namespace: `Moq.AutoMock`. [Documentation index](README.md).

## ObjectGraphContext

Tracks one construction graph for [resolvers](Resolvers.md).

```csharp
ObjectGraphContext(bool enablePrivate)
ObjectGraphContext(bool enablePrivate, bool isMockCreation)
ObjectGraphContext(ObjectGraphContext context)
ObjectGraphContext(ObjectGraphContext context, ParameterInfo targetParameter)
void AddDiagnosticMessage(string message)
```

`enablePrivate` includes non-public instance constructors. Copy constructors copy `BindingFlags` and clone `VisitedTypes`; they do not copy diagnostics or `IsMockCreation`. The parameter overload also sets `ParameterInfo`. Null copy sources or target parameters throw `ArgumentNullException`.

| Property | Type | Meaning |
| --- | --- | --- |
| `BindingFlags` | `BindingFlags` | Constructor search flags |
| `VisitedTypes` | `HashSet<Type>` | Mutable cycle-detection state |
| `IsMockCreation` | `bool` | Explicit mock creation request |
| `ParameterInfo` | `ParameterInfo?` | Constructor parameter currently being resolved, otherwise null |
| `DiagnosticMessages` | `IReadOnlyList<string>` | Messages recorded during resolution |

```csharp
var graph = new ObjectGraphContext(enablePrivate: false);
graph.AddDiagnosticMessage("Custom resolver could not provide the service.");
var copy = new ObjectGraphContext(graph);
var flags = copy.BindingFlags;
foreach (string message in graph.DiagnosticMessages)
    Console.WriteLine(message);
```

## ObjectCreationException

Thrown when a root instance or self-mock cannot be constructed. `DiagnosticMessages` describes failed resolution attempts.

```csharp
ObjectCreationException(IReadOnlyList<string> diagnosticMessages)
ObjectCreationException(string message, IReadOnlyList<string> diagnosticMessages)
ObjectCreationException(
    string message, Exception innerException, IReadOnlyList<string> diagnosticMessages)
protected ObjectCreationException(SerializationInfo info, StreamingContext context)
```

The public constructors require non-null diagnostics. The protected overload supports legacy exception serialization.

```csharp
try
{
    var service = mocker.CreateInstance<OrderService>();
}
catch (ObjectCreationException exception)
{
    Console.WriteLine(exception.Message);
    foreach (string message in exception.DiagnosticMessages)
        Console.WriteLine(message);
    throw;
}
```

## ConstructorTestsAttribute

Marks a partial test class for generated constructor null-argument tests. Apply once per class.

```csharp
ConstructorTestsAttribute()
ConstructorTestsAttribute(Type targetType)
ConstructorTestsAttribute(Type targetType, TestGenerationBehavior behavior)
Type? TargetType { get; set; }
TestGenerationBehavior Behavior { get; set; }
```

Set the target through the constructor or `TargetType`. `Behavior` defaults to `TestGenerationBehavior.Default`.

```csharp
using Moq.AutoMock;

[ConstructorTests(typeof(OrderService), TestGenerationBehavior.IgnoreNullableParameters)]
public partial class OrderServiceTests
{
}
```

Equivalent named-property form:

```csharp
[ConstructorTests(
    TargetType = typeof(OrderService),
    Behavior = TestGenerationBehavior.IgnoreNullableParameters)]
public partial class OrderServiceTests
{
}
```

## TestGenerationBehavior

| Value | Meaning |
| --- | --- |
| `Default` (`0`) | Generate null checks for all eligible parameters |
| `IgnoreNullableParameters` (`1`) | Skip nullable references, nullable value types, and parameters with a null default |

See [Unit Test Generator](SourceGenerators/UnitTestGenerator.md) for supported test frameworks, eligibility rules, and customization hooks.
