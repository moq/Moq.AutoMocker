using System;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock
{
    internal static class ConstructorSelector
    {
        public static ConstructorInfo SelectFor(Type type, Type[] existingTypes, BindingFlags bindingFlags)
        {
            ConstructorInfo best = null;
            foreach (var constructor in type.GetConstructors(bindingFlags))
            {
                if (IsBetterChoice(best, constructor, existingTypes))
                    best = constructor;
            }
            return best;
        }

        private static bool IsBetterChoice(ConstructorInfo current, ConstructorInfo candidate, Type[] existingTypes)
        {
            if (current == null)
                return true;

            if (candidate.GetParameters()
                         .Where(x => !existingTypes.Contains(x.ParameterType))
                         .Any(x => x.ParameterType.GetTypeInfo().IsSealed && !x.ParameterType.IsArray))
                return false;

            return current.GetParameters().Length < candidate.GetParameters().Length;
        }
    }
}