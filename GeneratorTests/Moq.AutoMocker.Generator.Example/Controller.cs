namespace Moq.AutoMock.Generator.Example;

public class Controller
{
    public IService Service { get; }

    public Controller(IService service)
    {
        Service = service ?? throw new ArgumentNullException(nameof(service));
    }
}
