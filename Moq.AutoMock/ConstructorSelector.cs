using System;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock
{
    internal static class ConstructorSelector
    {
        public static ConstructorInfo SelectCtor(this Type type, Type[] existingTypes, BindingFlags bindingFlags)
        {
            ConstructorInfo? best = type
                .GetConstructors(bindingFlags)
                .Aggregate<ConstructorInfo, ConstructorInfo?>(null, (value, constructor) =>
                {
                    if (value is null) return constructor;
                    if (value.GetParameters().Length >= constructor.GetParameters().Length)
                        return value;

                    if (constructor.GetParameters()
                        .Where(x => !existingTypes.Contains(x.ParameterType))
                        .Any(x => x.ParameterType.GetTypeInfo().IsSealed && !x.ParameterType.IsArray))
                        return value;

                    return constructor;
                });

            return best ?? Empty(type) ?? throw new ArgumentException($"Did not find a best constructor for `{type}`", nameof(type));

            static ConstructorInfo Empty(Type type) => type
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => !x.GetParameters().Any());
        }
    }
}