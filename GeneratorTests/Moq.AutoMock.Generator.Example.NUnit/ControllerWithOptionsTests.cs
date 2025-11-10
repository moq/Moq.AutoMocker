using NUnit.Framework;

namespace Moq.AutoMock.Generator.Example.NUnit;
public class ControllerWithOptionsTests
{
    [Test]
    public void CreateInstance_WithOptions_EmbedsOptions()
    {
        AutoMocker mocker = new();
        
        mocker.WithOptions<TestsOptions>(options => 
        {
            options.Number = 42;
            options.Required = "Some Value";
        });

        ControllerWithOptions controller = mocker.CreateInstance<ControllerWithOptions>();

        Assert.That(42 == controller.Options.Value.Number);
        Assert.That("Some Value" == controller.Options.Value.Required);
    }
}
