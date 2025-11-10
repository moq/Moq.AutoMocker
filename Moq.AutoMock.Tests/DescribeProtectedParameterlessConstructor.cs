namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeProtectedParameterlessConstructor
{
    [TestMethod]
    [Description("Issue 369")]
    public void It_should_use_protected_parameterless_constructor_when_public_constructor_has_unresolvable_parameter()
    {
        // Arrange
        var mocker = new AutoMocker();

        // Act - should not throw
        var instance = mocker.CreateInstance<ClassWithProtectedParameterlessConstructor>(enablePrivate: true);

        // Assert
        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ClassWithProtectedParameterlessConstructor));
    }

    [TestMethod]
    [Description("Issue 369")]
    public void It_should_resolve_dependency_with_protected_parameterless_constructor()
    {
        // Arrange
        var mocker = new AutoMocker();

        // Act - should not throw
        var instance = mocker.CreateInstance<ClassWithDependencyHavingProtectedConstructor>(enablePrivate: true);

        // Assert
        Assert.IsNotNull(instance);
        Assert.IsNotNull(instance.Dependency);
        Assert.IsInstanceOfType(instance, typeof(ClassWithDependencyHavingProtectedConstructor));
    }

    [TestMethod]
    [Description("Issue 369")]
    public void It_should_get_mock_with_protected_parameterless_constructor()
    {
        // Arrange
        var mocker = new AutoMocker();

        // Act - should not throw
        var mock = mocker.GetMock<ClassWithProtectedParameterlessConstructor>(enablePrivate: true);

        // Assert
        Assert.IsNotNull(mock);
        Assert.IsNotNull(mock.Object);
    }
}

// Test classes
public class ClassWithProtectedParameterlessConstructor
{
    public virtual Uri? Uri { get; }

    public ClassWithProtectedParameterlessConstructor(Uri uri)
    {
        Uri = uri;
    }

    protected ClassWithProtectedParameterlessConstructor()
    {
    }
}

public class ClassWithDependencyHavingProtectedConstructor(ClassWithProtectedParameterlessConstructor dependency)
{
    public ClassWithProtectedParameterlessConstructor Dependency { get; } = dependency;
}
