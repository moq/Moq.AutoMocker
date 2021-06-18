using Moq.AutoMock.Extensions;
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
                .Where(constructor =>
                {
                    var parameters = constructor.GetParameters();

                    return parameters.Length is 0 || parameters
                        .Select(parameter => parameter.ParameterType)
                        .All(type => existingTypes.Contains(type)
                            || type.IsMockable()
                            || type.IsArray);
                })
                .Aggregate<ConstructorInfo, ConstructorInfo?>(null, (value, constructor) =>
                {
                    if (value is null) return constructor;
                    return value.GetParameters().Length >= constructor.GetParameters().Length ? value : constructor;
                });

            return best 
                ?? Empty(type) 
                ?? throw new ArgumentException(
                    $"Did not find a best constructor for `{type}`. If your type has a non-public constructor, set the 'enablePrivate' parameter to true for this AutoMocker method.",
                    nameof(type));

            static ConstructorInfo Empty(Type type) => type
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(x => x.GetParameters().Length is 0);
        }
    }
}
