using System.Reflection;
using Xunit;

namespace Moq.AutoMock.Generator.DisableTests;

/// <summary>
/// These tests verify that when the MSBuild properties are set to disable the generators,
/// the generated extension methods are not present.
/// 
/// Each generator produces a static extension class with specific methods. When disabled,
/// these types should not exist in the current assembly (the test project's compilation).
/// The source generators add code to the consuming project, so we check the current assembly.
/// </summary>
public class GeneratorDisableTests
{
    private static readonly Assembly CurrentAssembly = typeof(GeneratorDisableTests).Assembly;

    [Fact]
    public void OptionsGenerator_WhenDisabled_ExtensionMethodDoesNotExist()
    {
        // When EnableMoqAutoMockerOptionsGenerator=false, the AutoMockerOptionsExtensions class
        // should not be generated in this assembly
        
        bool hasGeneratedOptionsExtension = HasExtensionMethod("WithOptions");
        Assert.False(hasGeneratedOptionsExtension, 
            "WithOptions extension method should not exist when EnableMoqAutoMockerOptionsGenerator=false");
    }

    [Fact]
    public void KeyedServicesGenerator_WhenDisabled_ExtensionMethodDoesNotExist()
    {
        // When EnableMoqAutoMockerKeyedServicesGenerator=false, the keyed services extension
        // methods should not be generated
        
        bool hasGeneratedKeyedExtension = HasExtensionMethod("WithKeyedService");
        Assert.False(hasGeneratedKeyedExtension,
            "WithKeyedService extension method should not exist when EnableMoqAutoMockerKeyedServicesGenerator=false");
    }

    [Fact]
    public void FakeLoggingGenerator_WhenDisabled_ExtensionMethodDoesNotExist()
    {
        // When EnableMoqAutoMockerFakeLoggingGenerator=false, the fake logging extension
        // methods should not be generated
        
        bool hasGeneratedLoggingExtension = HasExtensionMethod("WithFakeLogging");
        Assert.False(hasGeneratedLoggingExtension,
            "WithFakeLogging extension method should not exist when EnableMoqAutoMockerFakeLoggingGenerator=false");
    }

    [Fact]
    public void ApplicationInsightsGenerator_WhenDisabled_ExtensionMethodDoesNotExist()
    {
        // When EnableMoqAutoMockerApplicationInsightsGenerator=false, the Application Insights
        // extension methods should not be generated
        
        bool hasGeneratedAppInsightsExtension = HasExtensionMethod("WithApplicationInsights");
        Assert.False(hasGeneratedAppInsightsExtension,
            "WithApplicationInsights extension method should not exist when EnableMoqAutoMockerApplicationInsightsGenerator=false");
    }

    /// <summary>
    /// Checks if an extension method with the given name exists for the AutoMocker type
    /// in the current assembly (generated code is added to the consuming project).
    /// </summary>
    private static bool HasExtensionMethod(string methodName)
    {
        // Check all types in the current assembly for extension methods on AutoMocker
        foreach (var type in CurrentAssembly.GetTypes())
        {
            if (!type.IsSealed || !type.IsAbstract) // static classes are sealed and abstract
                continue;
            
            if (!type.Namespace?.StartsWith("Moq.AutoMock") ?? true)
                continue;

            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
            foreach (var method in methods)
            {
                if (method.Name == methodName)
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length > 0 && parameters[0].ParameterType == typeof(AutoMocker))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
