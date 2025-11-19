
namespace Moq.AutoMock.Generator.Example.MSTest;

[TestClass]
public class ControllerWithKeyedServiceTests
{
    [TestMethod]
    public void CreateInstance_WithKeyedService_ResolvesService()
    {
        AutoMocker mocker = new();

        IService service = Mock.Of<IService>();
        IService service2 = Mock.Of<IService>();
        mocker.WithKeyedService(service, "Test");
        mocker.WithKeyedService(service2, "Test2");

        ControllerWithKeyedService controller = mocker.CreateInstance<ControllerWithKeyedService>();

        Assert.AreEqual(service, controller.Service);
        Assert.AreEqual(service2, controller.Service2);
    }
}
