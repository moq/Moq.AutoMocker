# Compiler integration

Namespace/assembly: `Moq.AutoMocker.Generators`. [Documentation index](README.md).

These public types support Roslyn hosts and generator tests. Normal test projects consume the [generated extensions](GeneratedExtensions.md) and attributes instead; the package loads the generators automatically.

## Generators and analyzer

Each generator is sealed, has a public parameterless constructor, implements `IIncrementalGenerator`, and exposes `void Initialize(IncrementalGeneratorInitializationContext context)`:

| Type | Output |
| --- | --- |
| `UnitTestSourceGenerator` | Constructor null-argument tests |
| `OptionsExtensionSourceGenerator` | Options registration |
| `FakeLoggingExtensionSourceGenerator` | Fake logging registration |
| `FakeTimeProviderExtensionSourceGenerator` | Fake time registration |
| `ApplicationInsightsExtensionSourceGenerator` | Version-specific telemetry capture |
| `KeyedServicesExtensionSourceGenerator` | Keyed service registration and resolution |
| `MeterFactoryExtensionSourceGenerator` | Meter factory registration |
| `HttpClientFactoryResolverSourceGenerator` | Named HTTP client factory registration |

Roslyn calls `Initialize`; do not invoke it yourself.

```csharp
using Moq.AutoMocker.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

IIncrementalGenerator generator = new UnitTestSourceGenerator();
GeneratorDriver driver = CSharpGeneratorDriver.Create(generator.AsSourceGenerator());
// compilation is a CSharpCompilation containing test sources and references.
driver = driver.RunGenerators(compilation);
GeneratorDriverRunResult result = driver.GetRunResult();
```

`UseOptionsAnalyzer()` derives from `DiagnosticAnalyzer`. `SupportedDiagnostics` is an `ImmutableArray<DiagnosticDescriptor>` containing `AMA0001`. `void Initialize(AnalysisContext context)` registers analysis of `Use(IOptions<T>)` when `WithOptions` is available.

```csharp
var analyzer = new UseOptionsAnalyzer();
var descriptor = analyzer.SupportedDiagnostics[0];
```

## Diagnostics

`Diagnostics` contains static nested classes exposing `const string DiagnosticId` and a `Diagnostic Create(...)` factory:

| Nested class | ID | Severity | Factory |
| --- | --- | --- | --- |
| `TestClassesMustBePartial` | `AMG0001` | Error | `Create(Location? location, string className)` |
| `MustReferenceAutoMock` | `AMG0002` | Warning | `Create()` |
| `MustSpecifyTargetType` | `AMG0003` | Error | `Create(Location? location, string className)` |
| `DuplicateTargetType` | `AMG0004` | Warning | `Create(Location? location, string targetTypeName)` |
| `PreferWithOptions` | `AMA0001` | Info | `Create(Location? location, string optionsTypeName)` |

```csharp
Diagnostic diagnostic = Diagnostics.MustSpecifyTargetType.Create(
    Location.None, "OrderServiceTests");
string id = Diagnostics.MustSpecifyTargetType.DiagnosticId;
```

## Generator models

`GeneratorTargetClass()`, `SutClass()`, `NullConstructorParameterTest()`, and `SyntaxReceiver()` have public parameterless constructors.

| Type | Members |
| --- | --- |
| `GeneratorTargetClass` | Static get-only `Empty`; get/set `string? Namespace`, `string? TestClassName`, `SutClass? Sut`, `bool SkipNullableParameters`, `Location? AttributeLocation` |
| `SutClass` | Get/set `string? Name`, `string? FullName`; get-only mutable `List<NullConstructorParameterTest> NullConstructorParameterTests` |
| `NullConstructorParameterTest` | Get/set `List<Parameter>? Parameters`, `int NullParameterIndex`, `string? NullTypeName`; get-only `string? NullParameterName` from the indexed parameter |
| `Parameter` | Constructor `Parameter(IParameterSymbol symbol)`; get-only `bool IsValueType`, `bool IsNullable`, `string Name`, `string ParameterType` |
| `SyntaxReceiver` | Get-only mutable `List<GeneratorTargetClass> TestClasses`, `List<Diagnostic> DiagnosticMessages`; `void OnVisitSyntaxNode(GeneratorSyntaxContext context)` |

`Empty` is a shared mutable model; create a new model for independent state. `Parameter` snapshots the symbol's name, fully qualified type, value-type status, and nullable annotation/null default. `NullParameterIndex` must be valid when reading `NullParameterName` from a non-null parameter list.

```csharp
var target = new GeneratorTargetClass
{
    Namespace = "Example.Tests",
    TestClassName = "OrderServiceTests",
    SkipNullableParameters = true,
    Sut = new SutClass { Name = "OrderService", FullName = "global::Example.OrderService" }
};
// parameterSymbol is a constructor's IParameterSymbol.
var parameter = new Parameter(parameterSymbol);
target.Sut.NullConstructorParameterTests.Add(new NullConstructorParameterTest
{
    Parameters = new List<Parameter> { parameter },
    NullParameterIndex = 0,
    NullTypeName = parameter.ParameterType
});
```

`SyntaxReceiver` implements `ISyntaxContextReceiver`; Roslyn supplies syntax contexts to `OnVisitSyntaxNode`, which collects attributed test classes and diagnostics.

```csharp
var receiver = new SyntaxReceiver();
// In a Roslyn host callback:
receiver.OnVisitSyntaxNode(context);
var classes = receiver.TestClasses;
var diagnostics = receiver.DiagnosticMessages;
```

`TargetTestingFramework` values are `Unknown` (0), `MSTest` (1), `XUnit` (2), `NUnit` (3), and `TUnit` (4).

```csharp
TargetTestingFramework framework = TargetTestingFramework.XUnit;
```

## AutoMock constants

`AutoMock` exposes these `const string` fields:

| Field | Value |
| --- | --- |
| `ConstructorTestsAttribute` | `"ConstructorTestsAttribute"` |
| `TargetTypePropertyName` | `"TargetType"` |
| `AssemblyName` | `"Moq.AutoMock"` |
| `NuGetPackageName` | `"Moq.AutoMock"` |
| `IgnoreNullableParametersEnumValue` | `"IgnoreNullableParameters"` |

```csharp
string runtimeAssemblyName = AutoMock.AssemblyName;
```

The repository-only `Generators.CombineGenerator` also has a parameterless constructor and `Initialize(IncrementalGeneratorInitializationContext)`. It generates the runtime library's `Combine` overloads (2-11 type arguments), not consuming-project extensions.
