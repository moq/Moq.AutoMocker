using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests.Resolvers;

[TestClass]
public class SimpleTypeResolverTests : BaseResolverTests<SimpleTypeResolverTests.TestSimpleTypeResolver>
{
    [TestMethod]
    public void WhenRequestTypeMatchesExactlyThenResolves()
    {
        var mocker = new AutoMocker();

        var context = Resolve<TestDerived>(mocker);

        Assert.IsTrue(context.ValueProvided);
        Assert.IsInstanceOfType<TestDerived>(context.Value);
    }

    [TestMethod]
    public void WhenRequestTypeIsAssignableToTAndIncludeBaseTypesIsTrueThenResolves()
    {
        var mocker = new AutoMocker();
        var resolver = new DerivedSimpleTypeResolver { TestIncludeBaseTypes = true };

        var context = Resolve<TestDerived>(mocker, resolver);

        Assert.IsTrue(context.ValueProvided);
        Assert.IsInstanceOfType<TestBase>(context.Value);
    }

    [TestMethod]
    public void WhenRequestTypeIsAssignableToTAndIncludeBaseTypesIsFalseThenDoesNotResolve()
    {
        var mocker = new AutoMocker();
        var resolver = new DerivedSimpleTypeResolver { TestIncludeBaseTypes = false };

        var context = Resolve<TestDerived>(mocker, resolver);

        Assert.IsFalse(context.ValueProvided);
        Assert.IsNull(context.Value);
    }

    [TestMethod]
    public void WhenRequestTypeIsBaseTypeAndIncludeBaseTypesIsTrueThenDoesNotResolve()
    {
        var mocker = new AutoMocker();
        var resolver = new TestSimpleTypeResolver { TestIncludeBaseTypes = true };

        var context = Resolve<TestBase>(mocker, resolver);

        Assert.IsFalse(context.ValueProvided);
        Assert.IsNull(context.Value);
    }

    [TestMethod]
    public void WhenRequestTypeIsInterfaceAndIncludeInterfacesIsTrueThenResolves()
    {
        var mocker = new AutoMocker();
        var resolver = new TestSimpleTypeResolver { TestIncludeInterfaces = true };

        var context = Resolve<ITestInterface>(mocker, resolver);

        Assert.IsTrue(context.ValueProvided);
        Assert.IsInstanceOfType<TestDerived>(context.Value);
    }

    [TestMethod]
    public void WhenRequestTypeIsInterfaceAndIncludeInterfacesIsFalseThenDoesNotResolve()
    {
        var mocker = new AutoMocker();
        var resolver = new TestSimpleTypeResolver { TestIncludeInterfaces = false };

        var context = Resolve<ITestInterface>(mocker, resolver);

        Assert.IsFalse(context.ValueProvided);
        Assert.IsNull(context.Value);
    }

    [TestMethod]
    public void WhenRequestTypeIsUnrelatedThenDoesNotResolve()
    {
        var mocker = new AutoMocker();

        var context = Resolve<string>(mocker);

        Assert.IsFalse(context.ValueProvided);
        Assert.IsNull(context.Value);
    }

    [TestMethod]
    public void WhenBothIncludesAreFalseAndRequestTypeIsExactMatchThenResolves()
    {
        var mocker = new AutoMocker();
        var resolver = new TestSimpleTypeResolver { TestIncludeBaseTypes = false, TestIncludeInterfaces = false };

        var context = Resolve<TestDerived>(mocker, resolver);

        Assert.IsTrue(context.ValueProvided);
        Assert.IsInstanceOfType<TestDerived>(context.Value);
    }

    public interface ITestInterface { }

    public class TestBase { }

    public class TestDerived : TestBase, ITestInterface { }

    public class TestSimpleTypeResolver : SimpleTypeResolver<TestDerived>
    {
        public bool TestIncludeBaseTypes
        {
            get => IncludeBaseTypes;
            set => IncludeBaseTypes = value;
        }

        public bool TestIncludeInterfaces
        {
            get => IncludeInterfaces;
            set => IncludeInterfaces = value;
        }

        protected override TestDerived GetValue(MockResolutionContext context)
        {
            return new TestDerived();
        }
    }

    private class DerivedSimpleTypeResolver : SimpleTypeResolver<TestBase>
    {
        public bool TestIncludeBaseTypes
        {
            get => IncludeBaseTypes;
            set => IncludeBaseTypes = value;
        }

        public bool TestIncludeInterfaces
        {
            get => IncludeInterfaces;
            set => IncludeInterfaces = value;
        }

        protected override TestBase GetValue(MockResolutionContext context)
        {
            return new TestBase();
        }
    }
}
