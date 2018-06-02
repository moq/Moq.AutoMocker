using System;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock
{
    internal static class ConstructorSelector
    {
        public static ConstructorInfo SelectCtor(this Type type, Type[] existingTypes, BindingFlags bindingFlags)
        {
            ConstructorInfo best = null;
            foreach (var constructor in type.GetConstructors(bindingFlags))
            {
                if (isBetterChoice(constructor))
                    best = constructor;
            }
            return best;

            bool isBetterChoice(ConstructorInfo candidate)
            {
                if (best == null) return true;

                if (candidate.GetParameters()
                             .Where(x => !existingTypes.Contains(x.ParameterType))
                             .Any(x => x.ParameterType.GetTypeInfo().IsSealed && !x.ParameterType.IsArray))
                    return false;

                return best.GetParameters().Length < candidate.GetParameters().Length;
            }
        }
    }
}