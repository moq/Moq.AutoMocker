using Microsoft.Extensions.Options;
using Xunit;

namespace Moq.AutoMock.Generator.Example.xUnit;
public class ControllerWithOptionsTests
{
    [Fact]
    public void CreateInstance_WithOptions_EmbedsOptions()
    {
        AutoMocker mocker = new();
        
        mocker.WithOptions<TestsOptions>(options => options.Number = 42);

        ControllerWithOptions controller = mocker.CreateInstance<ControllerWithOptions>();

        Assert.Equal(42, controller.Options.Value.Number);
    }
}
