using Microsoft.Extensions.Time.Testing;

namespace Moq.AutoMock.Generator.Example.MSTest;

[TestClass]
public class ControllerWithTimeProviderTests
{
    [TestMethod]
    public void CreateInstance_WithFakeTimeProvider_CreatesController()
    {
        AutoMocker mocker = new();

        mocker.WithFakeTimeProvider();

        ControllerWithTimeProvider controller = mocker.CreateInstance<ControllerWithTimeProvider>();

        Assert.IsNotNull(controller);
    }

    [TestMethod]
    public void CreateInstance_WithFakeTimeProvider_UsesControlledTime()
    {
        AutoMocker mocker = new();

        mocker.WithFakeTimeProvider();

        var fakeTimeProvider = mocker.Get<FakeTimeProvider>();
        var startTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(startTime);

        ControllerWithTimeProvider controller = mocker.CreateInstance<ControllerWithTimeProvider>();

        Assert.AreEqual(startTime, controller.GetCurrentTime());

        fakeTimeProvider.Advance(TimeSpan.FromHours(1));

        Assert.AreEqual(startTime.AddHours(1), controller.GetCurrentTime());
    }
}
