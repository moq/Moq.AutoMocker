using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests.Resolvers;

[TestClass]
public class SelfResolverTests : BaseResolverTests<SelfResolver>
{
    [TestMethod]
    public void WhenRequestTypeIsAutoMockerThenResolvesToAutoMockerInstance()
    {
        var mocker = new AutoMocker();

        var context = Resolve<AutoMocker>(mocker);

        Assert.IsTrue(context.ValueProvided);
        Assert.AreSame(mocker, context.Value);
    }

    [TestMethod]
    public void WhenRequestTypeIsIServiceProviderThenResolvesToAutoMockerInstance()
    {
        var mocker = new AutoMocker();

        var context = Resolve<IServiceProvider>(mocker);

        Assert.IsTrue(context.ValueProvided);
        Assert.AreSame(mocker, context.Value);
    }

    [TestMethod]
    [Description("Issue 450")]
    public void WhenServiceProviderIsRequestedAsMockThenItReturnsMockedServiceProvider()
    {
        var mocker = new AutoMocker();

        var serviceProvider = mocker.GetMock<IServiceProvider>();

        Assert.IsNotNull(serviceProvider);
    }
}
