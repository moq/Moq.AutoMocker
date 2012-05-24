using Should;
using Xunit;

namespace Moq.AutoMock.Tests
{
    public class AutoMockerTests
    {
        #region Types used for testing

        public class Empty
        {
        }

        public class OneConstructor
        {
            public Empty Empty;

            public OneConstructor(Empty empty)
            {
                this.Empty = empty;
            }
        }

        public class WithService
        {
            public IService2 Service { get; set; }

            public WithService(IService2 service)
            {
                Service = service;
            }
        }

        #endregion

        public class DescribeGetInstance
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void It_creates_object_with_no_constructor()
            {
                var instance = mocker.GetInstance<Empty>();
                instance.ShouldNotBeNull();
            }

            [Fact]
            public void It_creates_objects_for_ctor_parameters()
            {
                var instance = mocker.GetInstance<OneConstructor>();
                instance.Empty.ShouldNotBeNull();
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_parameters()
            {
                var instance = mocker.GetInstance<OneConstructor>();
                Mock.Get(instance.Empty).ShouldNotBeNull();
            }
        }

        public class DescribeUsingExplicitObjects
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void You_can_Use_an_instance_as_an_argument_to_GetInstance()
            {
                var empty = new Empty();
                mocker.Use(empty);
                var instance = mocker.GetInstance<OneConstructor>();
                instance.Empty.ShouldBeSameAs(empty);
            }

            [Fact]
            public void You_can_use_Use_as_an_alias_for_MockOf()
            {
                mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
                var instance = mocker.GetInstance<WithService>();
                instance.Service.ShouldImplement<IService2>();
                instance.Service.Other.ShouldImplement<IService1>();
            }
        }

        public class DescribeExtractingObjects
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void It_extracts_instances_that_were_placed_with_Use()
            {
                var setupInstance = Mock.Of<IService1>();
                mocker.Use(setupInstance);

                var actualInstance = mocker.Extract<IService1>();
                actualInstance.ShouldBeSameAs(setupInstance);
            }
        }
    }
}
