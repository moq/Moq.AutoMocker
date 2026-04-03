using Xunit;

namespace Moq.AutoMock.Generator.Example.xUnit;

public class ControllerWithKeyedServiceTests
{
    [Fact]
    public void CreateInstance_WithKeyedService_ResolvesService()
    {
        AutoMocker mocker = new();

        IService service = Mock.Of<IService>();
        mocker.Use(service);
        IService service2 = Mock.Of<IService>();
        mocker.WithKeyedService<IService, KeyedServiceWithDependency>("Test");
        mocker.WithKeyedService(service2, "Test2");

        ControllerWithKeyedService controller = mocker.CreateInstance<ControllerWithKeyedService>();

        var keyedService = Assert.IsType<KeyedServiceWithDependency>(controller.Service);
        Assert.Equal(service, keyedService.Service);
        Assert.Equal(service2, controller.Service2);
    }
}
