using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Moq.AutoMock.Tests
{
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
            Assert.IsFalse(services.Any());
        }

        [TestMethod]
        public void It_creates_mock_objects_for_ctor_array_parameters_with_elements()
        {
            var mocker = new AutoMocker();
            mocker.Use(new Mock<IService2>());
            WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
            IService2[] services = instance.Services;
            Assert.IsNotNull(services);
            Assert.IsTrue(services.Any());
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
            StringAssert.Contains(exception.StackTrace, typeof(ConstructorThrows).Name);
        }
    }
}
