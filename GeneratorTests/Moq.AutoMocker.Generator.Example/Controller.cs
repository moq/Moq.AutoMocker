namespace Moq.AutoMock.Generator.Example;

public class Controller
{
    public Controller(IService service)
    {
        _ = service ?? throw new ArgumentNullException(nameof(service));
    }

    public Controller(IService service1, IService service2)
    {
        _ = service1 ?? throw new ArgumentNullException(nameof(service1));
        _ = service2 ?? throw new ArgumentNullException(nameof(service2));
    }

    public Controller(IService service, string name)
    {
        _ = service ?? throw new ArgumentNullException(nameof(service));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public Controller(ILogger<Controller> logger)
    {
        _ = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Controller(ILogger<Controller> logger, CancellationToken token)
    {
        _ = logger ?? throw new ArgumentNullException(nameof(logger));
        _ = token;
    }

    public string Name { get; } = "";
}
