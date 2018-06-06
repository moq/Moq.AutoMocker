using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using System;
using System.Collections.Generic;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class ResolvesDelegates
    {
        delegate string ZeroParamDelegate();
        delegate object OneParamDelegate(int p);
        delegate long TwoParamDelegate(int p, object foo);

        [DataTestMethod, DynamicData(nameof(Delegates))]
        public void ResolvesFuncReturningDefinedParameter(Type delegateType, object expected)
        {
            var mocker = new AutoMocker { Resolvers = { new DelegateResolver() } };
            mocker.Use(expected.GetType(), expected);

            var func = (Delegate)mocker.Get(delegateType);
            Assert.IsNotNull(func);

            var @params = new object[delegateType.GetMethod("Invoke").GetParameters().Length];
            Assert.AreEqual(expected, func.DynamicInvoke(@params));
        }
        static IEnumerable<object[]> Delegates
        {
            get
            {
                yield return new[] { typeof(Func<object>), new object() };
                yield return new[] { typeof(Func<string, object>), new object() };
                yield return new object[] { typeof(Func<object, string>), nameof(ResolvesDelegates) };
                yield return new object[] { typeof(Func<object, ResolvesDelegates, string>), nameof(ResolvesDelegates) };
                yield return new object[] { typeof(Func<string, int, Service2>), new Service2() };
                yield return new object[] { typeof(ZeroParamDelegate), nameof(Service2) };
                yield return new object[] { typeof(OneParamDelegate), new object() };
                yield return new object[] { typeof(TwoParamDelegate), 42L };
            }
        }
    }
}
