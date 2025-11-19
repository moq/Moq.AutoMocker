namespace Moq.AutoMock.Generator.Example.TUnit;

public class ControllerWithKeyedServiceTests
{
    [Test]
    public async Task CreateInstance_WithKeyedService_ResolvesService()
    {
        AutoMocker mocker = new();

        IService service = Mock.Of<IService>();
        IService service2 = Mock.Of<IService>();
        mocker.WithKeyedService(service, "Test");
        mocker.WithKeyedService(service2, "Test2");

        ControllerWithKeyedService controller = mocker.CreateInstance<ControllerWithKeyedService>();

        await Assert.That(controller.Service).IsEqualTo(service);
        await Assert.That(controller.Service2).IsEqualTo(service2);
    }
}
