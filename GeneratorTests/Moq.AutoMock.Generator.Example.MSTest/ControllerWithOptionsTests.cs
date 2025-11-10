namespace Moq.AutoMock.Generator.Example.MSUnit;
[TestClass]
public class ControllerWithOptionsTests
{
    [TestMethod]
    public void CreateInstance_WithOptions_EmbedsOptions()
    {
        AutoMocker mocker = new();
        
        mocker.WithOptions<TestsOptions>(options => 
        {
            options.Number = 42;
            options.Required = "Some Value";
        });

        ControllerWithOptions controller = mocker.CreateInstance<ControllerWithOptions>();

        Assert.AreEqual(42, controller.Options.Value.Number);
        Assert.AreEqual("Some Value", controller.Options.Value.Required);
    }
}
