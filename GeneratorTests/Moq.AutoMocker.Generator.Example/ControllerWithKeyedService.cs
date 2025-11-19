using Microsoft.Extensions.DependencyInjection;

namespace Moq.AutoMock.Generator.Example;

public class ControllerWithKeyedService
{
    public IService Service { get; }
    public IService Service2 { get; }

    public ControllerWithKeyedService([FromKeyedServices("Test")] IService service, 
        IServiceProvider serviceProvider)
    {
        Service = service;
        Service2 = serviceProvider.GetRequiredKeyedService<IService>("Test2");
    }
}

public class KeyedServiceWithDependency : IService
{
    public IService Service { get; }
    public KeyedServiceWithDependency(IService service)
    {
        Service = service;
    }
}
