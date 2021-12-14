using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests;

[TestClass]
public class ResolvesDelegates
{
    [DataTestMethod, DynamicData(nameof(DelegateTypes))]
    public void ResolvesFuncReturningDefinedParameter(Type delegateType, object expected)
    {
        var mocker = new AutoMocker { Resolvers = { new FuncResolver() } };
        mocker.Use(expected.GetType(), expected);

        var func = (Delegate)mocker.Get(delegateType)!;
        Assert.IsNotNull(func);

        var @params = new object[delegateType.GetGenericArguments().Length - 1];
        Assert.AreEqual(expected, func.DynamicInvoke(@params));
    }


    static IEnumerable<object[]> DelegateTypes
    {
        get
        {
            yield return new[] { typeof(Func<object>), new object() };
            yield return new[] { typeof(Func<string, object>), new object() };
            yield return new object[] { typeof(Func<object, string>), nameof(ResolvesDelegates) };
            yield return new object[] { typeof(Func<object, ResolvesDelegates, string>), nameof(ResolvesDelegates) };
            yield return new object[] { typeof(Func<string, int, Service2>), new Service2() };
        }
    }
}
