using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

/// <summary>
/// Based on https://gist.github.com/Keboo/d2f7f470d66fc4ff7d978ccb107b07cf
/// </summary>
namespace Moq.AutoMock.Tests
{
    public interface IConstructorTest
    {
        void Run();
    }

    [ExcludeFromCodeCoverage]
    public static class ConstructorTest
    {
        private class Test : IConstructorTest
        {
            public Dictionary<Type, object?> SpecifiedValues { get; }
                = new Dictionary<Type, object?>();

            private Type TargetType { get; }

            public Test(Type targetType)
            {
                TargetType = targetType;
            }

            public Test(Test original)
            {
                TargetType = original.TargetType;
                foreach (KeyValuePair<Type, object?> kvp in original.SpecifiedValues)
                {
                    SpecifiedValues[kvp.Key] = kvp.Value;
                }
            }

            public void Run()
            {
                foreach (ConstructorInfo constructor in TargetType.GetConstructors())
                {
                    ParameterInfo[] parameters = constructor.GetParameters();

                    object?[] parameterValues = parameters
                        .Select(p => p.ParameterType)
                        .Select(t =>
                        {
                            if (SpecifiedValues.TryGetValue(t, out object? value))
                            {
                                return value;
                            }
                            Mock mock = (Mock)Activator.CreateInstance(typeof(Mock<>).MakeGenericType(t))!;
                            return mock.Object;
                        })
                        .ToArray();
                }
            }
        }

        public static IConstructorTest Use<T>(this IConstructorTest test, T value)
        {
            if (test is Test internalTest)
            {
                var newTest = new Test(internalTest);
                newTest.SpecifiedValues.Add(typeof(T), value);
                return newTest;
            }
            else
            {
                throw new ArgumentException("Argument not expected type", nameof(test));
            }
        }

        public static IConstructorTest BuildArgumentNullExceptionsTest<T>()
            => new Test(typeof(T));

        public static void AssertArgumentNullExceptions<T>()
            => BuildArgumentNullExceptionsTest<T>().Run();
    }
}