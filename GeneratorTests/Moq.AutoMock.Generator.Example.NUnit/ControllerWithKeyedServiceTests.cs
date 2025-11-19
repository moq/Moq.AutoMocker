using NUnit.Framework;

namespace Moq.AutoMock.Generator.Example.NUnit;

public class ControllerWithKeyedServiceTests
{
    [Test]
    public void CreateInstance_WithKeyedService_ResolvesService()
    {
        AutoMocker mocker = new();

        IService service = Mock.Of<IService>();
        IService service2 = Mock.Of<IService>();
        mocker.WithKeyedService(service, "Test");
        mocker.WithKeyedService(service2, "Test2");

        ControllerWithKeyedService controller = mocker.CreateInstance<ControllerWithKeyedService>();

        Assert.That(controller.Service == service);
        Assert.That(controller.Service2 == service2);
    }
}
