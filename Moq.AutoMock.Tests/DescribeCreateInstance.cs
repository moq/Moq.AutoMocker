
using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeCreateInstance
{
    [TestMethod]
    public void It_creates_object_with_no_constructor()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<Empty>();
        Assert.IsNotNull(instance);
    }

    [TestMethod]
    public void It_creates_objects_for_ctor_parameters()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<OneConstructor>();
        Assert.IsNotNull(instance.Empty);
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_parameters()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<OneConstructor>();
        Assert.IsNotNull(Mock.Get(instance.Empty));
    }

    [TestMethod]
    public void It_creates_mock_objects_for_internal_ctor_parameters()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<WithServiceInternal>(true);
        Assert.IsNotNull(Mock.Get(instance.Service));
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_parameters_with_supplied_behavior()
    {
        var strictMocker = new AutoMocker(MockBehavior.Strict);

        var instance = strictMocker.CreateInstance<OneConstructor>();
        var mock = Mock.Get(instance.Empty);
        Assert.IsNotNull(mock);
        Assert.AreEqual(MockBehavior.Strict, mock.Behavior);
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_sealed_parameters_when_instances_provided()
    {
        var mocker = new AutoMocker();
        mocker.Use("Hello World");
        WithSealedParams instance = mocker.CreateInstance<WithSealedParams>();
        Assert.AreEqual("Hello World", instance.Sealed);
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_array_parameters()
    {
        var mocker = new AutoMocker();
        WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
        IService2[] services = instance.Services;
        Assert.IsNotNull(services);
        Assert.AreEqual(1, services.Length);
        Assert.IsTrue(services[0] is IService2);
    }

    [TestMethod]
    public void It_creates_mock_objects_for_ctor_array_parameters_with_elements()
    {
        var mocker = new AutoMocker();
        var expectedService = new Mock<IService2>();
        mocker.Use(expectedService);
        WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
        IService2[] services = instance.Services;
        Assert.IsNotNull(services);
        Assert.AreEqual(1, services.Length);
        Assert.AreEqual(expectedService.Object, services[0]);
    }

    [TestMethod]
    public void It_throws_original_exception_caught_whilst_creating_object()
    {
        var mocker = new AutoMocker();
        Assert.ThrowsException<ArgumentException>(mocker.CreateInstance<ConstructorThrows>);
    }

    [TestMethod]
    public void It_throws_original_exception_caught_whilst_creating_object_with_original_stack_trace()
    {
        var mocker = new AutoMocker();
        ArgumentException exception = Assert.ThrowsException<ArgumentException>(() => mocker.CreateInstance<ConstructorThrows>());
        StringAssert.Contains(exception.StackTrace!, typeof(ConstructorThrows).Name);
    }

    [TestMethod]
    public void It_creates_object_when_first_level_dependencies_are_classes()
    {
        var mocker = new AutoMocker();
        HasClassDependency instance = mocker.CreateInstance<HasClassDependency>();
        var dependency = instance.WithService;
        Assert.IsNotNull(dependency);
        Assert.IsInstanceOfType(dependency, typeof(WithService));
        Assert.IsInstanceOfType(Mock.Get(dependency), typeof(Mock<WithService>));
        Assert.AreSame(dependency, mocker.Get<WithService>());
    }

    [TestMethod]
    public void It_creates_object_with_2_first_level_dependencies()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<With2ClassDependencies>();

        var dependency1 = instance.WithService;
        Assert.IsNotNull(dependency1);
        Assert.IsInstanceOfType(dependency1, typeof(WithService));
        Assert.IsInstanceOfType(Mock.Get(dependency1), typeof(Mock<WithService>));
        Assert.AreSame(dependency1, mocker.Get<WithService>());

        var dependency2 = instance.With3Parameters;
        Assert.IsNotNull(dependency2);
        Assert.IsInstanceOfType(dependency2, typeof(With3Parameters));
        Assert.IsInstanceOfType(Mock.Get(dependency2), typeof(Mock<With3Parameters>));
    }

    [TestMethod]
    public void Second_level_dependencies_act_same_as_if_they_were_target()
    {
        var mocker = new AutoMocker();
        var instance = mocker.CreateInstance<HasFuncDependencies>();
        var dependency = instance.WithServiceFactory();
        Assert.IsNotNull(dependency);
        Assert.IsInstanceOfType(dependency, typeof(WithService));
        Assert.IsInstanceOfType(Mock.Get(dependency), typeof(Mock<WithService>));
        // Questionable if this is the correct behavior, but it is the current behavior.
        Assert.AreSame(dependency, mocker.Get<WithService>());
    }

    [TestMethod]
    public void It_throws_when_creating_object_with_recursive_dependency()
    {
        var mocker = new AutoMocker();
        // I could see this changing to something else in the future, like null. Right now, it seems
        // best to cause early failure to clarify what went wrong. Also, returning null "allows" the
        // behavior, so it's easier to move that direction later without breaking backward compatibility.
        ObjectCreationException e = Assert.ThrowsException<ObjectCreationException>(mocker.CreateInstance<WithRecursiveDependency>);
        Assert.IsTrue(e.Message.StartsWith($"Did not find a best constructor for `{typeof(WithRecursiveDependency)}`"));
    }

    [TestMethod]
    [Description("Issue 123")]
    public void It_can_use_fixed_value_to_supply_string_parameter()
    {
        AutoMocker mocker = new();
        mocker.Use("Test string");
        HasStringParameter sut = mocker.CreateInstance<HasStringParameter>();

        Assert.AreEqual("Test string", sut.String);
    }

    [TestMethod]
    [Description("Issue 123")]
    public void It_can_use_custom_resolver_to_supply_string_parameter()
    {
        AutoMocker mocker = new();
        mocker.Resolvers.Add(new CustomStringResolver("Test string"));
        HasStringParameter sut = mocker.CreateInstance<HasStringParameter>();

        Assert.AreEqual("Test string", sut.String);
    }

    [TestMethod]
    public void ConcreteDependencyFirst_WhenServiceIsShared_UsesResolvedInstance()
    {
        AutoMocker mocker = new();
        ConcreteDependencyIsFirst constructed = mocker.CreateInstance<ConcreteDependencyIsFirst>();

        Assert.AreSame(constructed.Service, constructed.Dependency.Service);
    }

    [TestMethod]
    public void ConcreteDependency_WhenServiceIsShared_UsesResolvedInstance()
    {
        AutoMocker mocker = new();
        ConcreteDependencyIsSecond constructed = mocker.CreateInstance<ConcreteDependencyIsSecond>();

        Assert.AreSame(constructed.Service, constructed.Dependency.Service);
    }

    [TestMethod]
    public void It_includes_reason_why_constructor_was_rejected()
    {
        AutoMocker mocker = new();

        ObjectCreationException ex = Assert.ThrowsException<ObjectCreationException>(() => mocker.CreateInstance<HasStringParameter>());

        Assert.AreEqual(1, ex.DiagnosticMessages.Count);
        Assert.AreEqual("Rejecting constructor Moq.AutoMock.Tests.DescribeCreateInstance+HasStringParameter(System.String string), because AutoMocker was unable to create parameter 'System.String string'", ex.DiagnosticMessages[0]);
    }

    [TestMethod]
    public void It_includes_reason_why_nested_constructor_was_rejected()
    {
        AutoMocker mocker = new();
        //Need to remove these resolvers to prevent AM from attempting to simply mock the values (which will throw a Moq exception), or create an instance
        mocker.Resolvers.Remove(mocker.Resolvers.OfType<MockResolver>().Single());

        ObjectCreationException ex = Assert.ThrowsException<ObjectCreationException>(() => mocker.CreateInstance<HasMultipleConstructors>());

        Assert.AreEqual(2, ex.DiagnosticMessages.Count);
        Assert.AreEqual("Rejecting constructor Moq.AutoMock.Tests.DescribeCreateInstance+HasMultipleConstructors(Moq.AutoMock.Tests.DescribeCreateInstance+HasMultipleConstructorsNested nested), because AutoMocker was unable to create parameter 'Moq.AutoMock.Tests.DescribeCreateInstance+HasMultipleConstructorsNested nested'", ex.DiagnosticMessages[0]);
        Assert.AreEqual("Rejecting constructor Moq.AutoMock.Tests.DescribeCreateInstance+HasMultipleConstructors(Moq.AutoMock.Tests.DescribeCreateInstance+HasStringParameter hasString), because AutoMocker was unable to create parameter 'Moq.AutoMock.Tests.DescribeCreateInstance+HasStringParameter hasString'", ex.DiagnosticMessages[1]);
    }

    [TestMethod]
    public void It_can_create_instances_of_nested_sealed_classes()
    {
        AutoMocker mocker = new();
        mocker.Resolvers.Add(new InstanceResolver());
        var mockWithSealedService = mocker.CreateInstance<HasNestedSealedService>();

        Assert.AreEqual(mockWithSealedService.SealedService, mockWithSealedService.NestedSealedService.SealedService);
    }

    private class CustomStringResolver : IMockResolver
    {
        public CustomStringResolver(string stringValue)
        {
            StringValue = stringValue;
        }

        public string StringValue { get; }

        public void Resolve(MockResolutionContext context)
        {
            if (context.RequestType == typeof(string))
            {
                context.Value = StringValue;
            }
        }
    }

    public class HasStringParameter
    {
        public HasStringParameter(string @string)
        {
            String = @string;
        }

        public string String { get; }
    }

    public class HasMultipleConstructors
    {
        public HasMultipleConstructors(HasMultipleConstructorsNested nested)
        {
            
        }

        public HasMultipleConstructors(HasStringParameter hasString)
        {
            
        }
    }

    public class HasMultipleConstructorsNested
    {
        public HasMultipleConstructorsNested(HasStringParameter hasString)
        {
            
        }
    }

    public class HasNestedSealedService
    {
        public SealedService SealedService { get; set; }
        public WithSealedService NestedSealedService { get; set; }

        public HasNestedSealedService(SealedService sealedService, WithSealedService nestedSealedService)
        {
            SealedService = sealedService;
            NestedSealedService = nestedSealedService;
        }
    }

    public record class ConcreteDependency(IService1 Service);
    public record class ConcreteDependencyIsFirst(ConcreteDependency Dependency, IService1 Service);
    public record class ConcreteDependencyIsSecond(IService1 Service, ConcreteDependency Dependency);
}
