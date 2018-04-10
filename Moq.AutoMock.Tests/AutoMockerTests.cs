using System;
using System.Reflection;
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
                Empty = empty;
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

        public class WithServiceInternal
        {
            public IService1 Service { get; set; }

            internal WithServiceInternal(IService1 service)
            {
                Service = service;
            }

            public WithServiceInternal() : this(null)
            {                
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

        private static Type MockVerificationException => typeof (Mock).GetTypeInfo().Assembly.GetType("Moq.MockVerificationException");

        public class DescribeCreateInstance
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void It_creates_object_with_no_constructor()
            {
                var instance = mocker.CreateInstance<Empty>();
                Assert.NotNull(instance);
            }

            [Fact]
            public void It_creates_objects_for_ctor_parameters()
            {
                var instance = mocker.CreateInstance<OneConstructor>();
                Assert.NotNull(instance.Empty);
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_parameters()
            {
                var instance = mocker.CreateInstance<OneConstructor>();
                Assert.NotNull(Mock.Get(instance.Empty));
            }

            [Fact]
            public void It_creates_mock_objects_for_internal_ctor_parameters()
            {
                var instance = mocker.CreateInstance<WithServiceInternal>(true);
                Assert.NotNull(Mock.Get(instance.Service));
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_parameters_with_supplied_behavior()
            {
                var strictMocker = new AutoMocker(MockBehavior.Strict);

                var instance = strictMocker.CreateInstance<OneConstructor>();
                var mock = Mock.Get(instance.Empty);
                Assert.NotNull(mock);
                Assert.Equal(MockBehavior.Strict, mock.Behavior);
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_sealed_parameters_when_instances_provided()
            {
                mocker.Use("Hello World");
                WithSealedParams instance = mocker.CreateInstance<WithSealedParams>();
                Assert.Equal("Hello World", instance.Sealed);
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_array_parameters()
            {
                WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
                IService2[] services = instance.Services;
                Assert.NotNull(services);
                Assert.Empty(services);
            }

            [Fact]
            public void It_creates_mock_objects_for_ctor_array_parameters_with_elements()
            {
                mocker.Use(new Mock<IService2>());
                WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
                IService2[] services = instance.Services;
                Assert.NotNull(services);
                Assert.NotEmpty(services);
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
                Assert.Same(empty, instance.Empty);
            }

            [Fact]
            public void You_can_use_Use_as_an_alias_for_MockOf()
            {
                mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
                var instance = mocker.CreateInstance<WithService>();
                Assert.IsAssignableFrom<IService2>(instance.Service);
                Assert.IsAssignableFrom<IService1>(instance.Service.Other);
            }

            [Fact]
            public void Adding_an_instance_will_replace_existing_setups()
            {
                mocker.Use<IService2>(x => x.Other.ToString() == "kittens");
                var otherService = Mock.Of<IService2>();
                mocker.Use(otherService);
                Assert.Same(otherService, mocker.Get<IService2>());
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
                mocker.Setup<IService2, IService1>(x => x.Other).Returns(Mock.Of<IService1>());
                var mock = mocker.Get<IService2>();
                Assert.NotNull(mock);
                Assert.NotNull(mock.Other);
            }

            [Fact]
            public void You_can_do_multiple_setups_on_a_single_interface()
            {
                mocker.Setup<IService2, IService1>(x => x.Other).Returns(Mock.Of<IService1>());
                mocker.Setup<IService2, string>(x => x.Name).Returns("pure awesomeness");
                var mock = mocker.Get<IService2>();
                Assert.Equal("pure awesomeness", mock.Name);
                Assert.NotNull(mock.Other);
            }

            [Fact]
            public void You_can_setup_a_void_void()
            {
                var x = 0;
                mocker.Setup<IService1>(_ => _.Void()).Callback(() => x++);
                mocker.Get<IService1>().Void();
                Assert.Equal(1, x);
            }

            [Fact]
            public void You_can_setup_a_method_that_returns_a_value_type()
            {
                mocker.Setup<IServiceWithPrimitives, long>(s => s.ReturnsALong()).Returns(100L);

                var mock = mocker.Get<IServiceWithPrimitives>();
                Assert.Equal(100L, mock.ReturnsALong());
            }

            [Fact]
            public void If_you_setup_a_method_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method without parameters
                var ex = Assert.Throws<NotSupportedException>(() => mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsALong()).Returns(100L));
                Assert.Equal("Use the Setup overload that allows specifying TReturn if the setup returns a value type", ex.Message);
            }

            [Fact]
            public void If_you_setup_a_method_with_a_parameter_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method with parameters
                var ex = Assert.Throws<NotSupportedException>(() => mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsALongWithParameter(It.IsAny<string>())).Returns(100L));

                Assert.Equal("Use the Setup overload that allows specifying TReturn if the setup returns a value type", ex.Message);

            }

            [Fact]
            public void If_you_setup_a_method_with_a_callback_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method with parameters
                var capturedVariable = string.Empty;

                var ex = Assert.Throws<NotSupportedException>(() => mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsALongWithParameter(It.IsAny<string>())).Returns(100L).Callback<string>(s => capturedVariable = s));

                Assert.Equal("Use the Setup overload that allows specifying TReturn if the setup returns a value type", ex.Message);

            }

            [Fact]
            public void If_you_setup_a_method_that_returns_a_value_type_via_a_lambda_without_specifying_return_type_you_get_useful_exception()
            {
                //a method with parameters
                var ex = Assert.Throws<NotSupportedException>(() => mocker.Setup<IServiceWithPrimitives>(s => s.ReturnsALongWithParameter(It.IsAny<string>())).Returns<string>(s => s.Length));

                Assert.Equal("Use the Setup overload that allows specifying TReturn if the setup returns a value type", ex.Message);

            }

            [Fact]
            public void You_can_setup_a_method_that_returns_a_reference_type_via_a_lambda_without_specifying_return_type()
            {
                //a method with parameters
                mocker.Setup<IServiceWithPrimitives, string>(s => s.ReturnsAReferenceWithParameter(It.IsAny<string>()))
                        .Returns<string>(s => s += "2");

                var mock = mocker.Get<IServiceWithPrimitives>();
                Assert.Equal("blah2", mock.ReturnsAReferenceWithParameter("blah"));
            }

            [Fact]
            public void You_can_setup_a_method_with_a_static_that_returns_a_reference_type_via_a_lambda_without_specifying_return_type()
            {
                //a method with parameters
                
                mocker.Setup<IService4, string>(s => s.MainMethodName(WithStatic.Get()))
                        .Returns<string>(s => s + "2");

                var mock = mocker.Get<IService4>();
                Assert.Equal("2", mock.MainMethodName(WithStatic.Get()));
            }

            [Fact]
            public void You_can_set_up_all_properties_with_one_line()
            {
                mocker.SetupAllProperties<IService5>();

                var mock = mocker.Get<IService5>();

                mock.Name = "aname";
                
                Assert.Equal("aname", mock.Name);
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

                Assert.Same(mocker.Get<IService2>(), mocker.Get<IService1>());
                Assert.Same(mocker.Get<IService3>(), mocker.Get<IService2>());
            }

            [Fact]
            public void Convenience_methods_work()
            {
                mocker.Combine<IService1, IService2, IService3>();

                Assert.Same(mocker.Get<IService2>(), mocker.Get<IService1>());
                Assert.Same(mocker.Get<IService3>(), mocker.Get<IService2>());
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
                Assert.Equal(100L, mock.ReturnsALong());

                mocker.Verify<IServiceWithPrimitives, long>(s => s.ReturnsALong(), Times.Once());
            }

            [Fact]
            public void You_can_verify_all_setups_marked_as_verifiable()
            {
                mocker.Setup<IService1>(x => x.Void()).Verifiable();
                mocker.Setup<IService5, string>(x => x.Name).Returns("Test");

                mocker.Get<IService1>().Void();
                
                mocker.Verify();
            }

            [Fact]
            public void If_you_verify_a_method_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method without parameters
                var ex = Assert.Throws<NotSupportedException>(() => mocker.Verify<IServiceWithPrimitives>(s => s.ReturnsALong(), Times.Once()));
                Assert.Equal("Use the Verify overload that allows specifying TReturn if the setup returns a value type", ex.Message);
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
                Assert.Same(setupInstance, actualInstance);
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
                Assert.NotNull(mock);
            }

            [Fact]
            public void It_creates_a_mock_if_the_object_is_missing_using_Get()
            {
                var mock = mocker.Get<IService1>();
                Assert.NotNull(mock);
            }

            [Fact]
            public void Mocks_that_were_autogenerated_can_be_extracted()
            {
                mocker.CreateInstance<WithService>();
                var actualInstance = mocker.Get<IService2>();
                Assert.NotNull(actualInstance);
            }

            [Fact]
            public void ExtractMock_throws_ArgumentException_when_object_isnt_A_mock()
            {
                mocker.Use<IService2>(new Service2());
                Assert.Throws<ArgumentException>(() => mocker.GetMock<IService2>());
            }
        }

        public class DescribeExtractingStrictObjects
        {
            private readonly AutoMocker mocker = new AutoMocker(MockBehavior.Strict);

            [Fact]
            public void It_creates_a_mock_as_strict_if_the_object_is_missing()
            {
                var mock = mocker.GetMock<IService1>();
                Assert.Equal(MockBehavior.Strict,mock.Behavior);
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
                Assert.False(selfMock.SelfDepricated);
            }

            [Fact]
            public void It_can_self_mock_objects_with_constructor_arguments()
            {
                var selfMock = mocker.CreateSelfMock<WithService>();
                Assert.NotNull(selfMock.Service);
                Assert.NotNull(Mock.Get(selfMock.Service));
            }
        }

        public class DescribeVerifyAll
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [Fact]
            public void It_calls_VerifyAll_on_all_objects_that_are_mocks()
            {
                mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
                var _ = mocker.CreateInstance<WithService>();
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
