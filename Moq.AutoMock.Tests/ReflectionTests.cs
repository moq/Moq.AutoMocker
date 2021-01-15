using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.Reflection.BindingFlags;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class ReflectionTests
    {
        [DataTestMethod]
        [DynamicData(nameof(DelegateTypes))]
        public void CheckAutoProperties(PropertyInfo property)
        {
            Type declaringType = property!.DeclaringType!;

            var mocker = new AutoMocker();
            mocker.Use(false);
            mocker.Use(string.Empty);

            try
            {
                var value = mocker.Get(declaringType);

                //These don't really matter, but this is the best way to
                //  exlude them from code coverage

                if (property.GetGetMethod() is { } getter)
                {
                    getter.Invoke(value, null);
                }

                if (property.GetSetMethod() is { } setter)
                {
                    setter.Invoke(value, new object?[] { null });
                }
            }
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }
        }

        private static IEnumerable<object[]> DelegateTypes => typeof(AutoMocker).Assembly.GetTypes()
            .Concat(typeof(ReflectionTests).Assembly.GetTypes())
            .Where(x => x != typeof(WithRecursiveDependency))
            .Where(x => !x.IsSealed && !x.IsInterface)
            .Where(x => x.GetCustomAttribute<ExcludeFromCodeCoverageAttribute>() is null)
            .SelectMany(x => from prop in x.GetProperties().Concat(x.GetProperties(NonPublic))
                             where prop.DeclaringType == x
                             select prop)
            .Where(x => x.GetCustomAttribute<ExcludeFromCodeCoverageAttribute>() is null)
            .Where(x => (x.GetGetMethod()?.GetCustomAttribute<CompilerGeneratedAttribute>()
                ?? x.GetSetMethod()?.GetCustomAttribute<CompilerGeneratedAttribute>()) is { })
            .Select(x => new object[] { x });
    }
}
