using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests.Resolvers;

[TestClass]
public class InsertResolverTests
{
    [TestMethod]
    public void InsertResolverAfter_WhenTargetResolverExists_InsertsAfterTarget()
    {
        var mocker = new AutoMocker();
        var originalResolvers = mocker.Resolvers.ToList();
        int selfResolverIndex = originalResolvers.FindIndex(r => r is SelfResolver);
        var newResolver = new TestResolver();

        mocker.InsertResolverAfter<SelfResolver>(newResolver);

        Assert.HasCount(originalResolvers.Count + 1, mocker.Resolvers);
        Assert.AreSame(newResolver, mocker.Resolvers[selfResolverIndex + 1]);
    }

    [TestMethod]
    public void InsertResolverAfter_WhenTargetIsLastResolver_InsertsAtEnd()
    {
        var mocker = new AutoMocker();
        var lastResolver = mocker.Resolvers[^1];
        int originalCount = mocker.Resolvers.Count;
        var newResolver = new TestResolver();

        Assert.IsInstanceOfType<MockResolver>(lastResolver);
        mocker.InsertResolverAfter<MockResolver>(newResolver);

        Assert.HasCount(originalCount + 1, mocker.Resolvers);
        Assert.AreSame(newResolver, mocker.Resolvers[^1]);
    }

    [TestMethod]
    public void InsertResolverAfter_WhenTargetNotFound_ThrowsInvalidOperationException()
    {
        var mocker = new AutoMocker();
        var newResolver = new TestResolver();

        var ex = Assert.Throws<InvalidOperationException>(
            () => mocker.InsertResolverAfter<TestResolver>(newResolver));

        Assert.Contains(nameof(TestResolver), ex.Message);
    }

    [TestMethod]
    public void InsertResolverAfter_WhenTargetIsFirstResolver_InsertsAtSecondPosition()
    {
        var mocker = new AutoMocker();
        var newResolver = new TestResolver();

        Assert.IsInstanceOfType<CacheResolver>(mocker.Resolvers[0]);
        mocker.InsertResolverAfter<CacheResolver>(newResolver);

        Assert.AreSame(newResolver, mocker.Resolvers[1]);
        Assert.IsInstanceOfType<CacheResolver>(mocker.Resolvers[0]);
    }

    [TestMethod]
    public void InsertResolverBefore_WhenTargetResolverExists_InsertsBeforeTarget()
    {
        var mocker = new AutoMocker();
        var originalResolvers = mocker.Resolvers.ToList();
        int mockResolverIndex = originalResolvers.FindIndex(r => r is MockResolver);
        var newResolver = new TestResolver();

        mocker.InsertResolverBefore<MockResolver>(newResolver);

        Assert.HasCount(originalResolvers.Count + 1, mocker.Resolvers);
        Assert.AreSame(newResolver, mocker.Resolvers[mockResolverIndex]);
        Assert.IsInstanceOfType<MockResolver>(mocker.Resolvers[mockResolverIndex + 1]);
    }

    [TestMethod]
    public void InsertResolverBefore_WhenTargetIsFirstResolver_InsertsAtBeginning()
    {
        var mocker = new AutoMocker();
        int originalCount = mocker.Resolvers.Count;
        var newResolver = new TestResolver();

        // CacheResolver is the first resolver in the default list
        mocker.InsertResolverBefore<CacheResolver>(newResolver);

        Assert.HasCount(originalCount + 1, mocker.Resolvers);
        Assert.AreSame(newResolver, mocker.Resolvers[0]);
        Assert.IsInstanceOfType<CacheResolver>(mocker.Resolvers[1]);
    }

    [TestMethod]
    public void InsertResolverBefore_WhenTargetNotFound_ThrowsInvalidOperationException()
    {
        var mocker = new AutoMocker();
        var newResolver = new TestResolver();

        var ex = Assert.Throws<InvalidOperationException>(
            () => mocker.InsertResolverBefore<TestResolver>(newResolver));

        Assert.Contains(nameof(TestResolver), ex.Message);
    }

    [TestMethod]
    public void InsertResolverBefore_WhenTargetIsLastResolver_InsertsBeforeLast()
    {
        var mocker = new AutoMocker();
        int originalCount = mocker.Resolvers.Count;
        var newResolver = new TestResolver();

        mocker.InsertResolverBefore<MockResolver>(newResolver);

        Assert.HasCount(originalCount + 1, mocker.Resolvers);
        Assert.AreSame(newResolver, mocker.Resolvers[^2]);
        Assert.IsInstanceOfType<MockResolver>(mocker.Resolvers[^1]);
    }

    private class TestResolver : IMockResolver
    {
        public void Resolve(MockResolutionContext context) { }
    }
}
