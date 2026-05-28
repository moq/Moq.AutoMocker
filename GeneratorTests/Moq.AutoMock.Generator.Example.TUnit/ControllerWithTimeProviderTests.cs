using Microsoft.Extensions.Time.Testing;

namespace Moq.AutoMock.Generator.Example.TUnit;

public partial class ControllerWithTimeProviderTests
{
    [Test]
    public async Task CreateInstance_WithFakeTimeProvider_CreatesController()
    {
        AutoMocker mocker = new();

        mocker.WithFakeTimeProvider();

        ControllerWithTimeProvider controller = mocker.CreateInstance<ControllerWithTimeProvider>();

        await Assert.That(controller).IsNotNull();
    }

    [Test]
    public async Task CreateInstance_WithFakeTimeProvider_UsesControlledTime()
    {
        AutoMocker mocker = new();

        mocker.WithFakeTimeProvider();

        var fakeTimeProvider = mocker.Get<FakeTimeProvider>();
        var startTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(startTime);

        ControllerWithTimeProvider controller = mocker.CreateInstance<ControllerWithTimeProvider>();

        await Assert.That(controller.GetCurrentTime()).IsEqualTo(startTime);

        fakeTimeProvider.Advance(TimeSpan.FromHours(1));

        await Assert.That(controller.GetCurrentTime()).IsEqualTo(startTime.AddHours(1));
    }
}
