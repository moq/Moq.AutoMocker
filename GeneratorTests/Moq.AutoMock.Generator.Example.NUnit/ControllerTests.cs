using Moq.AutoMock.Resolvers;
using NUnit.Framework;

namespace Moq.AutoMock.Generator.Example.NUnit;

public partial class ControllerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
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
        ArgumentNullException? ex = Assert.Throws<ArgumentNullException>(() => mocker.CreateInstance<Controller>());
        Assert.AreEqual("service", ex!.ParamName);
    }
}
