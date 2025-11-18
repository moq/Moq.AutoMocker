# Unit Test Generator

The Unit Test Generator automatically creates tests that verify your constructors properly validate null arguments. This helps ensure your classes properly guard against null dependencies.

## Features

- Automatically generates tests for each nullable constructor parameter
- Supports MSTest, xUnit, NUnit, and TUnit testing frameworks
- Validates that `ArgumentNullException` is thrown with the correct parameter name
- Provides setup hooks for test customization

## Basic Usage

To use the Unit Test Generator, decorate your test class with the `[ConstructorTests]` attribute and specify the target type:

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock;

[TestClass]
[ConstructorTests(TargetType = typeof(MyController))]
public partial class MyControllerTests
{
}
```

**Important:** Your test class **must** be declared as `partial` for the generator to work.

## Example

Given this class:

```csharp
public class MyController
{
    public MyController(IService service, ILogger<MyController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    private readonly IService _service;
    private readonly ILogger<MyController> _logger;
}
```

This test class:

```csharp
[TestClass]
[ConstructorTests(TargetType = typeof(MyController))]
public partial class MyControllerTests
{
}
```

Will automatically generate tests like:

```csharp
[TestMethod]
public void MyControllerConstructor_WithNullIService_ThrowsArgumentNullException()
{
    AutoMocker mocker = new AutoMocker();
    AutoMockerTestSetup(mocker, "MyControllerConstructor_WithNullIService_ThrowsArgumentNullException");
    MyControllerConstructor_WithNullIService_ThrowsArgumentNullExceptionSetup(mocker);
    using(IDisposable mockerDisposable = mocker.AsDisposable())
    {
        var logger = mocker.Get<ILogger<MyController>>();
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => _ = new MyController(null, logger));
        Assert.AreEqual("service", ex.ParamName);
    }
}

[TestMethod]
public void MyControllerConstructor_WithNullILogger_ThrowsArgumentNullException()
{
    // Similar test for logger parameter...
}
```

## Customization Hooks

The generator provides partial methods you can implement to customize test behavior:

### Global Setup Hook

Called before every generated test:

```csharp
[TestClass]
[ConstructorTests(TargetType = typeof(MyController))]
public partial class MyControllerTests
{
    partial void AutoMockerTestSetup(AutoMocker mocker, string testName)
    {
        // Common setup for all tests
        mocker.Use<string>("default value");
    }
}
```

### Per-Test Setup Hook

Called before a specific generated test:

```csharp
[TestClass]
[ConstructorTests(TargetType = typeof(MyController))]
public partial class MyControllerTests
{
    partial void MyControllerConstructor_WithNullIService_ThrowsArgumentNullExceptionSetup(AutoMocker mocker)
    {
        // Setup specific to this test
        mocker.Use<string>("specific value");
    }
}
```

## Ignoring Nullable Parameters

By default, the generator creates tests for all reference-type constructor parameters. If your class uses nullable reference types and you want to skip tests for nullable parameters, use the `Behavior` property:

```csharp
public class MyService
{
    public MyService(
        string name,           // Test will be generated
        string? nickname,      // No test generated (nullable)
        int? age = null)       // No test generated (nullable value type with default)
    {
        ArgumentNullException.ThrowIfNull(name);
    }
}

[TestClass]
[ConstructorTests(typeof(MyService), Behavior = TestGenerationBehavior.IgnoreNullableParameters)]
public partial class MyServiceTests
{
}
```

With `TestGenerationBehavior.IgnoreNullableParameters`, tests are skipped for parameters that:
- Are nullable reference types (when nullable reference types are enabled)
- Are nullable value types (e.g., `int?`)
- Have a default value of `null`

## Multiple Constructors

If your class has multiple constructors, the generator creates tests for parameters in all constructors:

```csharp
public class MyController
{
    public MyController(IService service)
    {
        // ...
    }
    
    public MyController(IService service1, IService service2)
    {
        // ...
    }
}
```

This will generate tests for all constructor parameter combinations that could be null.

## Supported Testing Frameworks

The Unit Test Generator automatically detects your testing framework and generates appropriate code:

| Framework | Detection | Attributes Used |
|-----------|-----------|-----------------|
| **MSTest** | References `Microsoft.VisualStudio.TestTools.UnitTesting` | `[TestMethod]` |
| **xUnit** | References `xunit.core` | `[Fact]` |
| **NUnit** | References `nunit.framework` | `[Test]` |
| **TUnit** | References `TUnit.Core` | `[Test]` (async) |

## Diagnostics

### AMG0001: Test class must be partial

**Error:** When using `[ConstructorTests]`, your test class must be declared as `partial`.

```csharp
// ❌ Error
[TestClass]
[ConstructorTests(TargetType = typeof(MyClass))]
public class MyClassTests // Missing 'partial' keyword
{
}

// ✅ Correct
[TestClass]
[ConstructorTests(TargetType = typeof(MyClass))]
public partial class MyClassTests
{
}
```

### AMG0002: Must reference Moq.AutoMock

**Warning:** Your test project must reference the Moq.AutoMock NuGet package to use the source generators.

```bash
dotnet add package Moq.AutoMock
```

### AMG0003: Must specify TargetType

**Error:** The `[ConstructorTests]` attribute must specify which type to generate tests for.

```csharp
// ❌ Error
[TestClass]
[ConstructorTests] // Missing TargetType
public partial class MyClassTests
{
}

// ✅ Correct
[TestClass]
[ConstructorTests(TargetType = typeof(MyClass))]
public partial class MyClassTests
{
}
```

### AMG0004: Duplicate target type detected

**Warning:** Multiple test classes targeting the same type may cause confusion.

```csharp
// ⚠️ Warning: Both classes test the same type
[ConstructorTests(TargetType = typeof(MyClass))]
public partial class MyClassTests1 { }

[ConstructorTests(TargetType = typeof(MyClass))]
public partial class MyClassTests2 { }
```

## Disabling the Generator

The Unit Test Generator only runs when you use the `[ConstructorTests]` attribute. Simply don't use the attribute to avoid generating constructor tests.

## Troubleshooting

### Generator Not Running

1. Ensure your project references `Moq.AutoMock`
2. Clean and rebuild your solution
3. Check for diagnostic errors in the Error List window
4. Verify the test class is declared as `partial`

### Tests Not Generated

1. Verify the `[ConstructorTests]` attribute has a `TargetType` specified
2. Check that the test class is `partial`
3. Ensure the target class has constructors with reference-type parameters
4. Review diagnostic warnings/errors in Visual Studio

## Best Practices

### Leverage Setup Hooks

Use the global setup hook for common configuration:

```csharp
partial void AutoMockerTestSetup(AutoMocker mocker, string testName)
{
    // Configure common dependencies
    mocker.Use<IConfiguration>(mockConfiguration);
}
```

### Keep Test Classes Organized

Consider one test class per target class for clarity:

```csharp
// MyService.cs
public class MyService { }

// MyServiceTests.cs
[ConstructorTests(TargetType = typeof(MyService))]
public partial class MyServiceTests { }
```

### Review Generated Code

To see the generated code, check your project's `obj` folder or enable source generators output:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```
