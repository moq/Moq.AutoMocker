using Xunit;

namespace Moq.AutoMock.Tests
{
	public class MockInstanceTests
	{
		public interface ISomeInterface
		{
		}

		[Fact]
		public void Instance_creates_mock_for_interface()
		{
			var instance = new MockInstance(typeof(ISomeInterface));
			Assert.NotNull(instance.Value);
			var mock = Assert.IsType<Mock<ISomeInterface>>(instance.Mock);
			Assert.NotNull(mock.Object);
		}

		public abstract class SomeAbstractClass
		{
		}

		[Fact]
		public void Instance_creates_mock_for_abstract_class()
		{
			var instance = new MockInstance(typeof(SomeAbstractClass));
			Assert.NotNull(instance.Value);
			var mock = Assert.IsType<Mock<SomeAbstractClass>>(instance.Mock);
			Assert.NotNull(mock.Object);
		}

		public class ClassWithDefaultConstructor1
		{
		}

		public class ClassWithDefaultConstructor2
		{
		}

		[Fact]
		public void Instance_creates_mock_for_class_with_default_constructor_only()
		{
			var instance = new MockInstance(typeof(ClassWithDefaultConstructor1));
			Assert.NotNull(instance.Value);
			var mock = Assert.IsType<Mock<ClassWithDefaultConstructor1>>(instance.Mock);
			Assert.NotNull(mock.Object);
		}

		public class ClassWithConstructorParametersWhoseClassesHaveDefaultConstructor
		{
			public ClassWithConstructorParametersWhoseClassesHaveDefaultConstructor(ClassWithDefaultConstructor1 class1, ClassWithDefaultConstructor2 class2)
			{
			}
		}

		[Fact]
		public void Instance_creates_mock_for_class_with_constructor_with_parameters_whose_classes_have_default_constructor()
		{
			var instance = new MockInstance(typeof(ClassWithConstructorParametersWhoseClassesHaveDefaultConstructor));
			Assert.NotNull(instance.Value);
			var mock = Assert.IsType<Mock<ClassWithConstructorParametersWhoseClassesHaveDefaultConstructor>>(instance.Mock);
			Assert.NotNull(mock.Object);
		}

		public class ClassWithConstructorWithAllKindsOfParameters
		{
			public ClassWithConstructorWithAllKindsOfParameters(
				ISomeInterface someInterface,
				SomeAbstractClass someAbstractClass,
				ClassWithDefaultConstructor1 class1,
				ClassWithDefaultConstructor1 class2,
				ClassWithConstructorParametersWhoseClassesHaveDefaultConstructor classWithParamsWithDefaultConstructors)
			{
			}
		}

		[Fact]
		public void Instance_creates_mock_for_class_with_constructor_with_all_kinds_of_parameters()
		{
			var instance = new MockInstance(typeof(ClassWithConstructorWithAllKindsOfParameters));
			Assert.NotNull(instance.Value);
			var mock = Assert.IsType<Mock<ClassWithConstructorWithAllKindsOfParameters>>(instance.Mock);
			Assert.NotNull(mock.Object);
		}
	}
}
