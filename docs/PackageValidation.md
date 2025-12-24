# Package Validation and Breaking Change Detection

This project uses [.NET Package Validation](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview) to detect breaking changes between versions.

## How It Works

The `Moq.AutoMock.csproj` file includes package validation settings:

```xml
<EnablePackageValidation>true</EnablePackageValidation>
<PackageValidationBaselineVersion>3.6.0</PackageValidationBaselineVersion>
```

When you run `dotnet pack`, the tooling compares the current build against the baseline version (3.6.0) to detect:
- **Breaking API changes** (removed or modified methods, properties, types)
- **Dropped target framework support**
- **Binary compatibility issues**

## When Validation Fails

If you encounter a `CP0002` error during `dotnet pack`, it means you've introduced a breaking change:

```
error CP0002: Member 'X.Y.Method(...)' exists on [Baseline] lib/netstandard2.0/Moq.AutoMock.dll but not on lib/netstandard2.0/Moq.AutoMock.dll
```

### For Unintentional Breaking Changes

Refactor your code to maintain backward compatibility:
- Add overloads instead of changing method signatures
- Mark members as `[Obsolete]` before removal
- Use optional parameters carefully (they can be binary breaking)

### For Intentional Breaking Changes

If you need to introduce a breaking change (e.g., for a major version release):

1. Generate a suppression file:
   ```bash
   dotnet pack /p:GenerateCompatibilitySuppressionFile=true
   ```

2. This creates/updates `CompatibilitySuppressions.xml` with entries for each breaking change:
   ```xml
   <Suppression>
     <DiagnosticId>CP0002</DiagnosticId>
     <Target>M:Moq.AutoMock.SomeClass.SomeMethod(...)</Target>
     <IsBaselineSuppression>true</IsBaselineSuppression>
   </Suppression>
   ```

3. Review and commit the `CompatibilitySuppressions.xml` file to document the breaking changes in your PR.

4. After releasing the new version, update the baseline:
   - Delete or clear `CompatibilitySuppressions.xml`
   - Update `PackageValidationBaselineVersion` to the new released version

## Updating the Baseline

After releasing a new version (e.g., 3.7.0), update the baseline in `Moq.AutoMock.csproj`:

```xml
<PackageValidationBaselineVersion>3.7.0</PackageValidationBaselineVersion>
```

This ensures future changes are validated against the latest stable release.

## Additional Resources

- [Baseline Version Validator Documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator)
- [Package Validation Overview](https://learn.microsoft.com/en-us/dotnet/fundamentals/apicompat/package-validation/overview)
- [Breaking Change Guidelines](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/breaking-changes)
