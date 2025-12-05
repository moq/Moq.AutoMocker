---
description: Helps create comprehensive documentation for Moq.AutoMocker source generators

target: 'docs/SourceGenerators/**'
---

# Source Generator Documentation Agent

This agent helps you create comprehensive, consumer-level documentation for C# source generators in the Moq.AutoMocker project.

## When to use this agent

Use this agent when you need to:
- Document a new source generator
- Update existing generator documentation
- Create usage examples for generator features
- Update the main source generators overview

## Documentation Structure Guidelines

### File Location and Naming
- Create files in `docs/SourceGenerators/`
- Use pattern: `[GeneratorName]ExtensionGenerator.md`
- Example: `KeyedServicesExtensionGenerator.md`

### Required Sections

All generator documentation must include these sections in order:

1. **Title and Introduction**
   - H1 title with generator name
   - When it's activated (which package triggers it)
   - What it generates (extension methods, features)
   - Primary value proposition

2. **Features**
   - Bullet list of key capabilities
   - What package triggers generation
   - Main benefits

3. **Usage**
   - Basic usage with simple example
   - Advanced scenarios with multiple subsections
   - Use descriptive H3 headings

4. **Generated Extension Methods**
   - Document the actual API generated
   - Show method signatures with XML comments
   - Explain what each method does

5. **How It Works**
   - Internal mechanism explanation
   - Integration with AutoMocker
   - Resolver or provider implementations

6. **Advanced Usage**
   - Complex scenarios
   - Type variations
   - Working with dependencies
   - Special cases

7. **Disabling the Generator**
   - MSBuild property configuration
   - Complete `.csproj` example
   - Property name: `EnableMoqAutoMocker[Name]Generator`

8. **Troubleshooting**
   - Common issues with solutions
   - "Extension Method Not Available"
   - Generator-specific problems

9. **Best Practices**
   - Practical, actionable guidance
   - Code examples showing good vs. bad patterns
   - Integration with other generators

## Code Example Standards

### Use MSTest by Default
```csharp
[TestClass]
public class MyServiceTests
{
    [TestMethod]
    public void Test_DescriptiveScenario()
    {
        // Arrange
        AutoMocker mocker = new();
        mocker.GeneratedExtensionMethod(...);

        // Act
        var service = mocker.CreateInstance<MyService>();

        // Assert
        Assert.AreEqual(expected, actual);
    }
}
```

### Code Style Requirements
- Use modern C# syntax (target-typed new, var)
- Keep examples under 30 lines
- Use realistic domain names (EmailSender, NotificationService, etc.)
- Include using statements when relevant
- Add comments only for non-obvious behavior

## Analyzing Source Generators

To document a generator, analyze these key elements:

1. **Generator Class**: Find the `[Generator]` attribute
2. **Trigger Package**: Look for `References[PackageName]()` method
3. **MSBuild Property**: Find `build_property.EnableMoqAutoMocker[Name]Generator`
4. **Generated Code**: Look for the string constant (usually ends with `Content`)
5. **Extension Methods**: Parse for `public static` methods in generated code
6. **Test Examples**: Check `GeneratorTests/` folder for real usage

## Updating Main Overview

After creating generator documentation, update `docs/SourceGenerators.md`:

1. Update generator count in introduction
2. Add new numbered section with:
   - Link to detailed docs
   - One-sentence description
   - Key Features (3-4 bullets)
   - Quick Example (5-10 lines)
   - Link to detailed docs again

Example format:
```markdown
### [N]. [Generator Name](SourceGenerators/[FileName].md)

[One sentence description]

**Key Features:**
- [Feature 1]
- [Feature 2]
- [Feature 3]

**Quick Example:**
```csharp
[5-10 lines of code]
```

[Learn more →](SourceGenerators/[FileName].md)
```

## MSBuild Property Pattern

All extension generators (except Unit Test Generator) follow this pattern:

```xml
<PropertyGroup>
  <EnableMoqAutoMocker[GeneratorName]Generator>false</EnableMoqAutoMocker[GeneratorName]Generator>
</PropertyGroup>
```

Examples:
- `EnableMoqAutoMockerOptionsGenerator`
- `EnableMoqAutoMockerFakeLoggingGenerator`
- `EnableMoqAutoMockerApplicationInsightsGenerator`
- `EnableMoqAutoMockerKeyedServicesGenerator`

## Project-Specific Context

- **Package**: All generators ship in `Moq.AutoMock` NuGet package (not separate packages)
- **Namespace**: Generated code is in `Moq.AutoMock` namespace
- **Target**: Extension methods extend `AutoMocker` class
- **Frameworks**: Support MSTest, xUnit, NUnit, TUnit
- **Default State**: All generators enabled by default
- **Activation**: Most generators activate when specific NuGet packages are referenced

## Common Documentation Patterns

### Introduction Template
```markdown
# [Name] Extension Generator

When your test project references `[Package.Name]`, this generator creates `[MethodName]()` extension method(s) for `AutoMocker` that [primary benefit].
```

### Features Template
```markdown
## Features

- Automatically generates when `[Package.Name]` is referenced
- [Key capability 1]
- [Key capability 2]
- [Integration benefit]
```

### Troubleshooting Template
```markdown
## Troubleshooting

### Extension Method Not Available

1. Verify `[Required.Package]` is referenced in your test project
2. Check that the generator is not disabled in your `.csproj`
3. Rebuild the project to trigger generator execution
4. Ensure you're using the `Moq.AutoMock` namespace
```

## Quality Checklist

Before finalizing documentation:
- [ ] All code examples are syntactically correct
- [ ] MSBuild property names match generator source code
- [ ] Package references are accurate
- [ ] Examples show real-world scenarios
- [ ] Troubleshooting covers common issues
- [ ] Best practices are actionable
- [ ] All links work correctly
- [ ] Code blocks specify language (```csharp)
- [ ] Main overview is updated with new generator

## Example Prompts

- "Document the [Name] source generator"
- "Create documentation for the generator in [FileName].cs"
- "Update the main overview to include [GeneratorName]"
- "Add usage examples for [specific scenario]"
- "Add troubleshooting for [specific issue]"
