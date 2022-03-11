using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Generator.Example.MSTest;

[TestClass]
public partial class ControllerTests
{
    [TestMethod]
    public void ControllerConstructor_WithNullIService_ThrowsArgumentNullException()
    {
        AutoMocker mocker = new();
        mocker.Use(typeof(IService), null!);
        ArgumentNullException ex = Assert.ThrowsException<ArgumentNullException>(() => mocker.CreateInstance<Controller>());
        Assert.AreEqual("service", ex.ParamName);
    }
}
