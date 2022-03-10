using Moq.AutoMock.Resolvers;
using Xunit;

namespace Moq.AutoMock.Generator.Example.xUnit;

public partial class ControllerTests
{
    [Fact]
    public void ControllerConstructor_WithNullIService_ThrowsArgumentNullException()
    {
        AutoMocker mocker = new();
        mocker.Resolvers.Insert(0, new GenericResolver(ctx =>
        {
            if (ctx.RequestType == typeof(IService))
            {
                ctx.Value = null;
            }
        }));
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => mocker.CreateInstance<Controller>());
        Assert.Equal("service", ex.ParamName);
    }
}
