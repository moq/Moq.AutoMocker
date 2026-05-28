using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

namespace Moq.AutoMock.Generator.Example.NUnit;

public class ControllerWithTimeProviderTests
{
    [Test]
    public void CreateInstance_WithFakeTimeProvider_CreatesController()
    {
        AutoMocker mocker = new();

        mocker.WithFakeTimeProvider();

        ControllerWithTimeProvider controller = mocker.CreateInstance<ControllerWithTimeProvider>();

        Assert.That(controller, Is.Not.Null);
    }

    [Test]
    public void CreateInstance_WithFakeTimeProvider_UsesControlledTime()
    {
        AutoMocker mocker = new();

        mocker.WithFakeTimeProvider();

        var fakeTimeProvider = mocker.Get<FakeTimeProvider>();
        var startTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(startTime);

        ControllerWithTimeProvider controller = mocker.CreateInstance<ControllerWithTimeProvider>();

        Assert.That(controller.GetCurrentTime(), Is.EqualTo(startTime));

        fakeTimeProvider.Advance(TimeSpan.FromHours(1));

        Assert.That(controller.GetCurrentTime(), Is.EqualTo(startTime.AddHours(1)));
    }
}
