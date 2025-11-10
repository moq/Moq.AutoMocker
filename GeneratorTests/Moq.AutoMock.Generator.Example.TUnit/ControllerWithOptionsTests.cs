namespace Moq.AutoMock.Generator.Example.TUnit;

public partial class ControllerWithOptionsTests
{
    [Test]
    public async Task CreateInstance_WithOptions_EmbedsOptions()
    {
        AutoMocker mocker = new();

        mocker.WithOptions<TestsOptions>(options => options.Number = 42);

        ControllerWithOptions controller = mocker.CreateInstance<ControllerWithOptions>();

        await Assert.That(controller.Options.Value.Number).IsEqualTo(42);
    }
}
