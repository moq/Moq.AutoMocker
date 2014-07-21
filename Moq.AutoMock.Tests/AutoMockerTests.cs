using System;
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

        public class WithStatic
        {
            public static string Get()
            {
                return string.Empty;
            }
        }

        public class ConstructorThrows
        {
            public ConstructorThrows()
            {
                throw new ArgumentException();
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

            [Fact]
            public void It_throws_original_exception_caught_whilst_creating_object()
            {
                Assert.Throws<ArgumentException>(() => mocker.CreateInstance<ConstructorThrows>());
            }

            [Fact]
            public void It_throws_original_exception_caught_whilst_creating_object_with_original_stack_trace()
            {
                ArgumentException exception = Assert.Throws<ArgumentException>(() => mocker.CreateInstance<ConstructorThrows>());
                Assert.Contains(typeof(ConstructorThrows).Name, exception.StackTrace);
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

            [Fact]
            public void You_can_setup_a_method_that_returns_a_value_type()
            {
                mocker.Setup<IServiceWithPrimitives, long>(s => s.ReturnsALong()).Returns(100L);

                var mock = mocker.Get<IServiceWithPrimitives>();
                mock.ReturnsALong().ShouldEqual(100L);
            }

            [Fact]
            public void If_you_setup_a_method_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method without parameters
                var ex = Assert.Throws<NotSupportedException>(() => mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsALong()).Returns(100L));
                ex.Message.ShouldEqual("Use the Setup overload that allows specifying TReturn if the setup returns a value type");
            }

            [Fact]
            public void If_you_setup_a_method_with_a_parameter_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method with parameters
                var ex = Assert.Throws<NotSupportedException>(() => mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsALongWithParameter(It.IsAny<string>())).Returns(100L));

                ex.Message.ShouldEqual("Use the Setup overload that allows specifying TReturn if the setup returns a value type");

            }

            [Fact]
            public void If_you_setup_a_method_with_a_callback_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method with parameters
                var capturedVariable = string.Empty;

                var ex = Assert.Throws<NotSupportedException>(() => mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsALongWithParameter(It.IsAny<string>())).Returns(100L).Callback<string>(s => capturedVariable = s));

                ex.Message.ShouldEqual("Use the Setup overload that allows specifying TReturn if the setup returns a value type");

            }

            [Fact]
            public void If_you_setup_a_method_that_returns_a_value_type_via_a_lambda_without_specifying_return_type_you_get_useful_exception()
            {
                //a method with parameters
                var ex = Assert.Throws<NotSupportedException>(() => mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsALongWithParameter(It.IsAny<string>())).Returns<string>(s => s.Length));

                ex.Message.ShouldEqual("Use the Setup overload that allows specifying TReturn if the setup returns a value type");

            }

            [Fact]
            public void You_can_setup_a_method_that_returns_a_reference_type_via_a_lambda_without_specifying_return_type()
            {
                //a method with parameters
                mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsAReferenceWithParameter(It.IsAny<string>()))
                        .Returns<string>(s => s += "2");

                var mock = mocker.Get<IServiceWithPrimitives>();
                mock.ReturnsAReferenceWithParameter("blah").ShouldEqual("blah2");
            }

            [Fact]
            public void You_can_setup_a_method_with_a_static_that_returns_a_reference_type_via_a_lambda_without_specifying_return_type()
            {
                //a method with parameters
                
                mocker.Setup<IService4>(s => s.MainMethodName(WithStatic.Get()))
                        .Returns<string>(s => s += "2");

                var mock = mocker.Get<IService4>();
                mock.MainMethodName(WithStatic.Get()).ShouldEqual("2");
            }

            [Fact]
            public void You_can_set_up_all_properties_with_one_line()
            {
                mocker.SetupAllProperties<IService5>();

                var mock = mocker.Get<IService5>();

                mock.Name = "aname";
                
                mock.Name.ShouldEqual("aname");
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

            [Fact]
            public void You_can_verify_a_method_that_returns_a_value_type()
            {
                mocker.Setup<IServiceWithPrimitives, long>(s => s.ReturnsALong()).Returns(100L);

                var mock = mocker.Get<IServiceWithPrimitives>();
                mock.ReturnsALong().ShouldEqual(100L);

                mocker.Verify<IServiceWithPrimitives, long>(s => s.ReturnsALong(), Times.Once());
            }

            [Fact]
            public void If_you_verify_a_method_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method without parameters
                var ex = Assert.Throws<NotSupportedException>(() => mocker.Verify<IServiceWithPrimitives>(s => s.ReturnsALong(), Times.Once()));
                ex.Message.ShouldEqual("Use the Verify overload that allows specifying TReturn if the setup returns a value type");
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
            public void It_creates_a_mock_if_the_object_is_missing_using_Get()
            {
                var mock = mocker.Get<IService1>();
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
    }
}
