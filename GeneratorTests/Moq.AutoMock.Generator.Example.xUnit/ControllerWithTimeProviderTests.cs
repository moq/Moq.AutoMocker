using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Moq.AutoMock.Generator.Example.xUnit;

public class ControllerWithTimeProviderTests
{
    [Fact]
    public void CreateInstance_WithFakeTimeProvider_CreatesController()
    {
        AutoMocker mocker = new();

        mocker.WithFakeTimeProvider();

        ControllerWithTimeProvider controller = mocker.CreateInstance<ControllerWithTimeProvider>();

        Assert.NotNull(controller);
    }

    [Fact]
    public void CreateInstance_WithFakeTimeProvider_UsesControlledTime()
    {
        AutoMocker mocker = new();

        mocker.WithFakeTimeProvider();

        var fakeTimeProvider = mocker.Get<FakeTimeProvider>();
        var startTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);
        fakeTimeProvider.SetUtcNow(startTime);

        ControllerWithTimeProvider controller = mocker.CreateInstance<ControllerWithTimeProvider>();

        Assert.Equal(startTime, controller.GetCurrentTime());

        fakeTimeProvider.Advance(TimeSpan.FromHours(1));

        Assert.Equal(startTime.AddHours(1), controller.GetCurrentTime());
    }
}
