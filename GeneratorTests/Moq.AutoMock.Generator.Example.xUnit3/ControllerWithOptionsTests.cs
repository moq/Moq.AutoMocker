using Xunit;

namespace Moq.AutoMock.Generator.Example.xUnit3;
public class ControllerWithOptionsTests
{
    [Fact]
    public void CreateInstance_WithOptions_EmbedsOptions()
    {
        AutoMocker mocker = new();

        mocker.WithOptions<TestsOptions>(options =>
        {
            options.Number = 42;
            options.Required = "Some Value";
        });

        ControllerWithOptions controller = mocker.CreateInstance<ControllerWithOptions>();

        Assert.Equal(42, controller.Options.Value.Number);
        Assert.Equal("Some Value", controller.Options.Value.Required);
    }
}
