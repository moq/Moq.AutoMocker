using Microsoft.CodeAnalysis;

namespace Moq.AutoMocker.Generators;

[Generator]
public class OptionsExtensionSourceGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        if (!ReferencesOptions(context.Compilation.ReferencedAssemblyNames))
        {
            return;
        }

        context.AddSource("AutoMocker.Options.cs", OptionsExtensionContent);
    }

    public void Initialize(GeneratorInitializationContext context)
    { }

    private static bool ReferencesOptions(IEnumerable<AssemblyIdentity> assemblies)
    {
        foreach (AssemblyIdentity assembly in assemblies)
        {
            if (assembly.Name.StartsWith("Microsoft.Extensions.Options"))
            {
                return true;
            }
        }
        return false;
    }

    private const string OptionsExtensionContent =
        """
        namespace Moq.AutoMock
        {
            using Microsoft.Extensions.Options;

            public static class AutoMockerOptionsExtensions
            {
                /// <summary>
                ///  This method sets up <see cref="AutoMocker"/> with various option related services for Microsoft's Option pattern, and allows their interception and manipulation in testing scenarios.
                /// </summary>
                /// <param name="mocker">The <see cref="AutoMocker"/> instance</param>
                /// <param name="configure">A delegate that can be used to configure an option instance of type TClass.</param>
                /// <typeparam name="TClass">The type of Options being configured.</typeparam>
                /// <returns>The same <see cref="AutoMocker"/> instance passed as parameter, allowing chained calls.</returns>
                public static AutoMocker WithOptions<TClass>(this AutoMocker mocker, Action<TClass>? configure = null)
                    where TClass : class, new()
                {
                    if (mocker == null) throw new ArgumentNullException(nameof(mocker));

                    mocker.Use<IEnumerable<IConfigureOptions<TClass>>>(new[] { new ConfigureOptions<TClass>(configure) });
                    mocker.With<IOptionsMonitorCache<TClass>, OptionsCache<TClass>>();
                    mocker.With<IOptionsFactory<TClass>, OptionsFactory<TClass>>();
                    mocker.With<IOptionsMonitor<TClass>, OptionsMonitor<TClass>>();
                    mocker.With<IOptionsSnapshot<TClass>, OptionsManager<TClass>>();
                    TClass options = mocker.Get<IOptionsFactory<TClass>>().Create(string.Empty);
                    mocker.Use(Options.Create(options));
                    mocker.Use(options);
                    return mocker;
                }
            }
        }
        """;
}
