using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeVerifyAll
{
    [TestMethod]
    public void It_calls_VerifyAll_on_all_objects_that_are_mocks()
    {
        var mocker = new AutoMocker();
        mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
        var _ = mocker.CreateInstance<WithService>();
        var ex = Assert.Throws<MockException>(() => mocker.VerifyAll());
        Assert.IsTrue(ex.IsVerificationError);
    }

    [TestMethod]
    public void It_doesnt_call_VerifyAll_if_the_object_isnt_a_mock()
    {
        var mocker = new AutoMocker();
        mocker.Use<IService2>(new Service2());
        mocker.CreateInstance<WithService>();
        mocker.VerifyAll(ignoreMissingSetups:true);
    }

    [TestMethod]
    public void It_throws_if_cache_is_not_registered()
    {
        AutoMocker mocker = new();
        mocker.Resolvers.Remove(mocker.Resolvers.OfType<CacheResolver>().Single());

        Assert.Throws<InvalidOperationException>(() => mocker.VerifyAll());
    }

    [TestMethod]
    public void It_throws_if_there_are_no_mocks_with_setups()
    {
        AutoMocker mocker = new();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => mocker.VerifyAll());
        Assert.AreEqual("VerifyAll was called, but there were no setups on any tracked mock instances to verify", ex.Message);
    }

    [TestMethod]
    public void It_does_not_throw_on_missing_setups_when_flag_is_set()
    {
        AutoMocker mocker = new();

        mocker.VerifyAll(ignoreMissingSetups: true);
    }
}
