namespace Moq.AutoMock.Generator.Example.MSTest;

[TestClass]
public class ControllerWithKeyedServiceTests
{
    [TestMethod]
    public void CreateInstance_WithKeyedService_ResolvesService()
    {
        AutoMocker mocker = new();

        IService service = Mock.Of<IService>();
        mocker.Use(service);
        IService service2 = Mock.Of<IService>();
        mocker.WithKeyedService<IService, KeyedServiceWithDependency>("Test");
        mocker.WithKeyedService(service2, "Test2");

        ControllerWithKeyedService controller = mocker.CreateInstance<ControllerWithKeyedService>();

        var keyedService = Assert.IsInstanceOfType<KeyedServiceWithDependency>(controller.Service);
        Assert.AreEqual(service, keyedService.Service);
        Assert.AreEqual(service2, controller.Service2);
    }
}
