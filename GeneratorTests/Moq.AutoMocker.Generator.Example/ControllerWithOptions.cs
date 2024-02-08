using Microsoft.Extensions.Options;

namespace Moq.AutoMock.Generator.Example;
public class ControllerWithOptions
{
    public IOptions<TestsOptions> Options { get; }

    public ControllerWithOptions(IOptions<TestsOptions> options)
    {
        Options = options;
    }
}
