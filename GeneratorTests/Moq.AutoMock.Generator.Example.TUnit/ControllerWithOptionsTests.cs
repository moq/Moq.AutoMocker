namespace Moq.AutoMock.Generator.Example.TUnit;

public partial class ControllerWithOptionsTests
{
    [Test]
    public async Task CreateInstance_WithOptions_EmbedsOptions()
    {
        AutoMocker mocker = new();

        mocker.WithOptions<TestsOptions>(options =>
        {
            options.Number = 42;
            options.Required = "Some Value";
        });

        ControllerWithOptions controller = mocker.CreateInstance<ControllerWithOptions>();

        await Assert.That(controller.Options.Value.Number).IsEqualTo(42);
        await Assert.That(controller.Options.Value.Required).IsEqualTo("Some Value");
    }
}
