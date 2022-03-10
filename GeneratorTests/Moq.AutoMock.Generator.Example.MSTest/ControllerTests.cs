using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Generator.Example.MSTest;

[TestClass]
public partial class ControllerTests
{
    [TestMethod]
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
        ArgumentNullException ex = Assert.ThrowsException<ArgumentNullException>(() => mocker.CreateInstance<Controller>());
        Assert.AreEqual("service", ex.ParamName);
    }
}
