using NUnit.Framework;

namespace Moq.AutoMock.Generator.Example.NUnit;

public class ControllerWithKeyedServiceTests
{
    [Test]
    public void CreateInstance_WithKeyedService_ResolvesService()
    {
        AutoMocker mocker = new();

        IService service = Mock.Of<IService>();
        mocker.Use(service);
        IService service2 = Mock.Of<IService>();
        mocker.WithKeyedService<IService, KeyedServiceWithDependency>("Test");
        mocker.WithKeyedService(service2, "Test2");

        ControllerWithKeyedService controller = mocker.CreateInstance<ControllerWithKeyedService>();

        Assert.That(controller.Service, Is.InstanceOf<KeyedServiceWithDependency>());
        var keyedService = (KeyedServiceWithDependency)controller.Service;
        Assert.That(keyedService.Service, Is.EqualTo(service));
        Assert.That(controller.Service2, Is.EqualTo(service2));
    }
}
