﻿using System;
using Should;
using Xunit;
using System.Reflection;

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

        public class WithServiceArray
        {
            public IService2[] Services { get; set; }

            public WithServiceArray(IService2[] services)
            {
                Services = services;
            }
        }

        public class WithSealedParams
        {
            public string Sealed { get; set; }

            public WithSealedParams(string @sealed)
            {
                Sealed = @sealed;
            }
        }

        public class InsecureAboutSelf
        {
            public bool SelfDepricated { get; set; }

            public void TellJoke()
            {
                
            }

            protected virtual void SelfDepricate()
            {
                SelfDepricated = true;
            }
        }

        #endregion

        private static Type MockVerificationException
        {
            get { return typeof (Mock).Assembly.GetType("Moq.MockVerificationException"); }
        }

        public class DescribeCreateInstance
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void It_creates_object_with_no_constructor()
            {
                var instance = mocker.CreateInstance<Empty>();
                instance.ShouldNotBeNull();
            }

            [Fact]
            public void It_creates_objects_for_ctor_parameters()
            {
                var instance = mocker.CreateInstance<OneConstructor>();
                instance.Empty.ShouldNotBeNull();
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_parameters()
            {
                var instance = mocker.CreateInstance<OneConstructor>();
                Mock.Get(instance.Empty).ShouldNotBeNull();
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_sealed_parameters_when_instances_provided()
            {
                mocker.Use("Hello World");
                WithSealedParams instance = mocker.CreateInstance<WithSealedParams>();
                instance.Sealed.ShouldNotBeNull().ShouldEqual("Hello World");
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_array_parameters()
            {
                WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
                instance.Services.ShouldNotBeNull().ShouldBeEmpty();
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_array_parameters_with_elements()
            {
                mocker.Use(new Mock<IService2>());
                WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
                instance.Services.ShouldNotBeNull().ShouldNotBeEmpty();
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
                var instance = mocker.CreateInstance<OneConstructor>();
                instance.Empty.ShouldBeSameAs(empty);
            }

            [Fact]
            public void You_can_use_Use_as_an_alias_for_MockOf()
            {
                mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
                var instance = mocker.CreateInstance<WithService>();
                instance.Service.ShouldImplement<IService2>();
                instance.Service.Other.ShouldImplement<IService1>();
            }

            [Fact]
            public void Adding_an_instance_will_replace_existing_setups()
            {
                mocker.Use<IService2>(x => x.Other.ToString() == "kittens");
                var otherService = Mock.Of<IService2>();
                mocker.Use(otherService);
                mocker.Get<IService2>().ShouldBeSameAs(otherService);
            }
        }

        public class DescribeSetups
        {
            private readonly AutoMocker mocker = new AutoMocker();

            /// <summary>
            /// Some people find this more comfortable than the Mock.Of() style
            /// </summary>
            [Fact]
            public void You_can_setup_a_mock_using_the_classic_Setup_style()
            {
                mocker.Setup<IService2>(x => x.Other).Returns(Mock.Of<IService1>());
                var mock = mocker.Get<IService2>();
                mock.ShouldNotBeNull();
                mock.Other.ShouldNotBeNull();
            }

            [Fact]
            public void You_can_do_multiple_setups_on_a_single_interface()
            {
                mocker.Setup<IService2>(x => x.Other).Returns(Mock.Of<IService1>());
                mocker.Setup<IService2>(x => x.Name).Returns("pure awesomeness");
                var mock = mocker.Get<IService2>();
                mock.Name.ShouldEqual("pure awesomeness");
                mock.Other.ShouldNotBeNull();
            }

            [Fact]
            public void You_can_setup_a_void_void()
            {
                var x = 0;
                mocker.Setup<IService1>(_ => _.Void()).Callback(() => x++);
                mocker.Get<IService1>().Void();
                x.ShouldEqual(1);
            }
        }

        public class DescribeCombiningTypes
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void It_uses_the_same_mock_for_all_instances()
            {
                mocker.Combine(typeof(IService1), typeof(IService2), 
                    typeof(IService3));

                mocker.Get<IService1>().ShouldBeSameAs(
                    mocker.Get<IService2>());
                mocker.Get<IService2>().ShouldBeSameAs(
                    mocker.Get<IService3>());
            }

            [Fact]
            public void Convenience_methods_work()
            {
                mocker.Combine<IService1, IService2, IService3>();

                mocker.Get<IService1>().ShouldBeSameAs(
                    mocker.Get<IService2>());
                mocker.Get<IService2>().ShouldBeSameAs(
                    mocker.Get<IService3>());
            }
        }

        public class DescribeSingleVerify
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void You_can_verify_a_single_method_call_directly()
            {
                var mock = new Mock<IService2>();
                mocker.Use(mock);
                var name = mock.Object.Name;
                mocker.Verify<IService2>(x => x.Name);
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

                var actualInstance = mocker.Get<IService1>();
                actualInstance.ShouldBeSameAs(setupInstance);
            }

            [Fact]
            public void It_extracts_instances_that_were_setup_with_Use()
            {
                mocker.Use<IService1>(x => x.ToString() == "");
                // Assert does not throw
                mocker.GetMock<IService1>();
            }

            [Fact]
            public void It_creates_a_mock_if_the_oject_is_missing()
            {
                var mock = mocker.GetMock<IService1>();
                mock.ShouldNotBeNull();
            }

            [Fact]
            public void Mocks_that_were_autogenerated_can_be_extracted()
            {
                mocker.CreateInstance<WithService>();
                var actualInstance = mocker.Get<IService2>();
                actualInstance.ShouldNotBeNull();
            }

            [Fact]
            public void ExtractMock_throws_ArgumentException_when_object_isnt_A_mock()
            {
                mocker.Use<IService2>(new Service2());
                Assert.Throws<ArgumentException>(() => mocker.GetMock<IService2>());
            }
        }

        public class DescribeCreatingSelfMocks
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void Self_mocks_are_useful_for_testing_most_of_class()
            {
                var selfMock = mocker.CreateSelfMock<InsecureAboutSelf>();
                selfMock.TellJoke();
                selfMock.SelfDepricated.ShouldBeFalse();
            }

            [Fact]
            public void It_can_self_mock_objects_with_constructor_arguments()
            {
                var selfMock = mocker.CreateSelfMock<WithService>();
                selfMock.Service.ShouldNotBeNull();
                Mock.Get(selfMock.Service).ShouldNotBeNull();
            }
        }

        public class DescribeVerifyAll
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void It_calls_VerifyAll_on_all_objects_that_are_mocks()
            {
                mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
                var selfMock = mocker.CreateInstance<WithService>();
                Assert.Throws(MockVerificationException, () => mocker.VerifyAll());
            }

            [Fact]
            public void It_doesnt_call_VerifyAll_if_the_object_isnt_a_mock()
            {
                mocker.Use<IService2>(new Service2());
                mocker.CreateInstance<WithService>();
                mocker.VerifyAll();
            }
        }

	    public class DescribeCreatingMocksForProperties
	    {
		    private AutoMocker mocker = new AutoMocker();

		    public class Property1
		    {
		    }

		    public class Property2
		    {
		    }

		    public interface IProperty3
		    {
		    }

		    public class ClassWithPublicProperties
		    {
			    public Property1 Property1 { get; set; }
			    public Property2 Property2 { get; private set; }
			    public IProperty3 Property3 { get; set; }
		    }

		    [Fact]
		    public void It_creates_mocks_for_properties_with_public_getter_and_setter()
		    {
			    var instance = mocker.CreateInstance<ClassWithPublicProperties>(Assembly.GetExecutingAssembly());
			    Assert.NotNull(instance.Property1);
			    Assert.Null(instance.Property2);
			    Assert.NotNull(instance.Property3);

				Assert.Equal(mocker.GetMock<Property1>().Object, instance.Property1);
				Assert.Equal(mocker.GetMock<IProperty3>().Object, instance.Property3);
		    }
	    }
    }
}
