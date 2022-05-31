namespace Moq.AutoMock.Tests;

[TestClass]
public class ResolvesCancellationToken
{
    [TestMethod]
    public void Resolves_cancellation_token_none_when_nothing_has_been_registered()
    {
        AutoMocker mocker = new();

        CancellationToken token = mocker.Get<CancellationToken>();

        Assert.AreEqual(CancellationToken.None, token);
    }

    [TestMethod]
    public void Resolves_cancellation_token_with_cached_instance()
    {
        using CancellationTokenSource cts = new();
        AutoMocker mocker = new();
        mocker.Use(cts.Token);

        CancellationToken token = mocker.Get<CancellationToken>();

        Assert.AreEqual(cts.Token, token);
    }

    [TestMethod]
    public void Resolves_cancellation_token_from_cached_cancellation_token_source()
    {
        using CancellationTokenSource cts = new();
        AutoMocker mocker = new();
        mocker.Use(cts);

        CancellationToken token = mocker.Get<CancellationToken>();

        Assert.AreEqual(cts.Token, token);
    }
}
