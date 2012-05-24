using System;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock
{
    internal class ConstructorSelector
    {
        public ConstructorInfo SelectFor(Type type)
        {
            ConstructorInfo best = null;
            foreach (var constructor in type.GetConstructors())
            {
                if (IsBetterChoice(best, constructor))
                    best = constructor;
            }
            return best;
        }

        private bool IsBetterChoice(ConstructorInfo current, ConstructorInfo candidate)
        {
            if (candidate.GetParameters().Any(x => x.ParameterType.IsSealed))
                return false;

            return current == null || current.GetParameters().Length < candidate.GetParameters().Length;
        }
    }
}