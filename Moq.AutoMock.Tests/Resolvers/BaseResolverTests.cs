using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests.Resolvers;

public abstract class BaseResolverTests<TResolver> where TResolver : IMockResolver, new()
{
    protected static MockResolutionContext Resolve<TRequestedType>(AutoMocker mocker)
        => Resolve<TRequestedType>(mocker, new TResolver());

    protected static MockResolutionContext Resolve<TRequestedType>(AutoMocker mocker, IMockResolver resolver)
        => Resolve(mocker, typeof(TRequestedType), resolver);

    protected static MockResolutionContext Resolve(AutoMocker mocker, Type requestedType, IMockResolver resolver)
    {
        var context = new MockResolutionContext(mocker, requestedType, new ObjectGraphContext(false));
        resolver.Resolve(context);
        return context;
    }
}
