namespace Moq.AutoMock.Generator.Example;

public class ControllerWithTimeProvider
{
    private readonly TimeProvider _timeProvider;

    public ControllerWithTimeProvider(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public DateTimeOffset GetCurrentTime() => _timeProvider.GetUtcNow();
}
