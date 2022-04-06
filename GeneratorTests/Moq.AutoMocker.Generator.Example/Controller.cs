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
}
