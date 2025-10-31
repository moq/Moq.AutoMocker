using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeCombiningTypes
{
    [TestMethod]
    public void It_uses_the_same_mock_for_all_instances()
    {
        var mocker = new AutoMocker();
        mocker.Combine(typeof(IService1), typeof(IService2),
            typeof(IService3));

        Assert.AreSame<object>(mocker.Get<IService2>(), mocker.Get<IService1>());
        Assert.AreSame<object>(mocker.Get<IService3>(), mocker.Get<IService2>());
    }

    [TestMethod]
    public void Convenience_methods_work()
    {
        var mocker = new AutoMocker();
        mocker.Combine<IService1, IService2, IService3>();

        Assert.AreSame<object>(mocker.Get<IService2>(), mocker.Get<IService1>());
        Assert.AreSame<object>(mocker.Get<IService3>(), mocker.Get<IService2>());
    }

    [TestMethod]
    [Description("Issue 107")]
    public void Combine_method_should_maintain_setups()
    {
        var mocker = new AutoMocker(MockBehavior.Loose);
        mocker.GetMock<IDerivedInterface>().Setup(x => x.Foo()).Returns(() => "42");
        mocker.Combine(typeof(IDerivedInterface), typeof(IBaseInterface));

        Assert.AreEqual("42", mocker.Get<IBaseInterface>().Foo());
        Assert.AreEqual("42", mocker.Get<IDerivedInterface>().Foo());
    }

    [TestMethod]
    public void Combine_throws_on_null_type()
    {
        AutoMocker mocker = new();
        var e = Assert.Throws<ArgumentNullException>(() => mocker.Combine(null!));
        Assert.AreEqual("type", e.ParamName);
    }

    [TestMethod]
    public void It_throws_if_cache_is_not_registered()
    {
        AutoMocker mocker = new();
        mocker.Resolvers.Remove(mocker.Resolvers.OfType<CacheResolver>().Single());

        Assert.Throws<InvalidOperationException>(() => mocker.Combine(typeof(object), typeof(object)));
    }
}

public interface IBaseInterface
{
    string Foo();
}

public interface IDerivedInterface : IBaseInterface
{
}
