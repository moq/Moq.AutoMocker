using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

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

        [TestClass]
        public class DescribeCreateInstance
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void It_creates_object_with_no_constructor()
            {
                var instance = mocker.CreateInstance<Empty>();
                Assert.IsNotNull(instance);
            }

            [TestMethod]
            public void It_creates_objects_for_ctor_parameters()
            {
                var instance = mocker.CreateInstance<OneConstructor>();
                Assert.IsNotNull(instance.Empty);
            }

            [TestMethod]
            public void It_creates_mock_objects_for_ctor_parameters()
            {
                var instance = mocker.CreateInstance<OneConstructor>();
                Assert.IsNotNull(Mock.Get(instance.Empty));
            }

            [TestMethod]
            public void It_creates_mock_objects_for_internal_ctor_parameters()
            {
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
                mocker.Use("Hello World");
                WithSealedParams instance = mocker.CreateInstance<WithSealedParams>();
                Assert.AreEqual("Hello World", instance.Sealed);
            }

            [TestMethod]
            public void It_creates_mock_objects_for_ctor_array_parameters()
            {
                WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
                IService2[] services = instance.Services;
                Assert.IsNotNull(services);
                Assert.IsFalse(services.Any());
            }

            [TestMethod]
            public void It_creates_mock_objects_for_ctor_array_parameters_with_elements()
            {
                mocker.Use(new Mock<IService2>());
                WithServiceArray instance = mocker.CreateInstance<WithServiceArray>();
                IService2[] services = instance.Services;
                Assert.IsNotNull(services);
                Assert.IsTrue(services.Any());
            }

            [TestMethod]
            public void It_throws_original_exception_caught_whilst_creating_object()
            {
                Assert.ThrowsException<ArgumentException>(mocker.CreateInstance<ConstructorThrows>);
            }

            [TestMethod]
            public void It_throws_original_exception_caught_whilst_creating_object_with_original_stack_trace()
            {
                ArgumentException exception = Assert.ThrowsException<ArgumentException>(() => mocker.CreateInstance<ConstructorThrows>());
                StringAssert.Contains(exception.StackTrace, typeof(ConstructorThrows).Name);
            }
        }

        [TestClass]
        public class DescribeUsingExplicitObjects
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void You_can_Use_an_instance_as_an_argument_to_GetInstance()
            {
                var empty = new Empty();
                mocker.Use(empty);
                var instance = mocker.CreateInstance<OneConstructor>();
                Assert.AreSame(empty, instance.Empty);
            }

            [TestMethod]
            public void You_can_use_Use_as_an_alias_for_MockOf()
            {
                mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
                var instance = mocker.CreateInstance<WithService>();
                Assert.IsInstanceOfType(instance.Service, typeof(IService2));
                Assert.IsInstanceOfType(instance.Service.Other, typeof(IService1));
            }

            [TestMethod]
            public void Adding_an_instance_will_replace_existing_setups()
            {
                mocker.Use<IService2>(x => x.Other.ToString() == "kittens");
                var otherService = Mock.Of<IService2>();
                mocker.Use(otherService);
                Assert.AreSame(otherService, mocker.Get<IService2>());
            }
        }

        [TestClass]
        public class DescribeSetups
        {
            private readonly AutoMocker mocker = new AutoMocker();

            /// <summary>
            /// Some people find this more comfortable than the Mock.Of() style
            /// </summary>
            [TestMethod]
            public void You_can_setup_a_mock_using_the_classic_Setup_style()
            {
                mocker.Setup<IService2, IService1>(x => x.Other).Returns(Mock.Of<IService1>());
                var mock = mocker.Get<IService2>();
                Assert.IsNotNull(mock);
                Assert.IsNotNull(mock.Other);
            }

            [TestMethod]
            public void You_can_do_multiple_setups_on_a_single_interface()
            {
                mocker.Setup<IService2, IService1>(x => x.Other).Returns(Mock.Of<IService1>());
                mocker.Setup<IService2, string>(x => x.Name).Returns("pure awesomeness");
                var mock = mocker.Get<IService2>();
                Assert.AreEqual("pure awesomeness", mock.Name);
                Assert.IsNotNull(mock.Other);
            }

            [TestMethod]
            public void You_can_setup_a_void_void()
            {
                var x = 0;
                mocker.Setup<IService1>(_ => _.Void()).Callback(() => x++);
                mocker.Get<IService1>().Void();
                Assert.AreEqual(1, x);
            }

            [TestMethod]
            public void You_can_setup_a_method_that_returns_a_value_type()
            {
                mocker.Setup<IServiceWithPrimitives, long>(s => s.ReturnsALong()).Returns(100L);

                var mock = mocker.Get<IServiceWithPrimitives>();
                Assert.AreEqual(100L, mock.ReturnsALong());
            }

            [TestMethod]
            public void You_can_setup_a_method_that_returns_a_reference_type_via_a_lambda_without_specifying_return_type()
            {
                //a method with parameters
                mocker.Setup<IServiceWithPrimitives, string>(s => s.ReturnsAReferenceWithParameter(It.IsAny<string>()))
                        .Returns<string>(s => s += "2");

                var mock = mocker.Get<IServiceWithPrimitives>();
                Assert.AreEqual("blah2", mock.ReturnsAReferenceWithParameter("blah"));
            }

            [TestMethod]
            public void You_can_setup_a_method_with_a_static_that_returns_a_reference_type_via_a_lambda_without_specifying_return_type()
            {
                //a method with parameters
                
                mocker.Setup<IService4, string>(s => s.MainMethodName(WithStatic.Get()))
                        .Returns<string>(s => s + "2");

                var mock = mocker.Get<IService4>();
                Assert.AreEqual("2", mock.MainMethodName(WithStatic.Get()));
            }

            [TestMethod]
            public void You_can_set_up_all_properties_with_one_line()
            {
                mocker.SetupAllProperties<IService5>();

                var mock = mocker.Get<IService5>();

                mock.Name = "aname";
                
                Assert.AreEqual("aname", mock.Name);
            }
        }

        [TestClass]
        public class DescribeCombiningTypes
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void It_uses_the_same_mock_for_all_instances()
            {
                mocker.Combine(typeof(IService1), typeof(IService2), 
                    typeof(IService3));

                Assert.AreSame(mocker.Get<IService2>(), mocker.Get<IService1>());
                Assert.AreSame(mocker.Get<IService3>(), mocker.Get<IService2>());
            }

            [TestMethod]
            public void Convenience_methods_work()
            {
                mocker.Combine<IService1, IService2, IService3>();

                Assert.AreSame(mocker.Get<IService2>(), mocker.Get<IService1>());
                Assert.AreSame(mocker.Get<IService3>(), mocker.Get<IService2>());
            }
        }

        [TestClass]
        public class DescribeSingleVerify
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void You_can_verify_a_single_method_call_directly()
            {
                var mock = new Mock<IService2>();
                mocker.Use(mock);
                var name = mock.Object.Name;
                mocker.Verify<IService2>(x => x.Name);
            }

            [TestMethod]
            public void You_can_verify_a_method_that_returns_a_value_type()
            {
                mocker.Setup<IServiceWithPrimitives, long>(s => s.ReturnsALong()).Returns(100L);

                var mock = mocker.Get<IServiceWithPrimitives>();
                Assert.AreEqual(100L, mock.ReturnsALong());

                mocker.Verify<IServiceWithPrimitives, long>(s => s.ReturnsALong(), Times.Once());
            }

            [TestMethod]
            public void You_can_verify_all_setups_marked_as_verifiable()
            {
                mocker.Setup<IService1>(x => x.Void()).Verifiable();
                mocker.Setup<IService5, string>(x => x.Name).Returns("Test");

                mocker.Get<IService1>().Void();
                
                mocker.Verify();
            }

            [TestMethod]
            public void If_you_verify_a_method_that_returns_a_value_type_without_specifying_return_type_you_get_useful_exception()
            {
                //a method without parameters
                var ex = Assert.ThrowsException<NotSupportedException>(() => mocker.Verify<IServiceWithPrimitives>(s => s.ReturnsALong(), Times.Once()));
                Assert.AreEqual("Use the Verify overload that allows specifying TReturn if the setup returns a value type", ex.Message);
            }
        }

        [TestClass]
        public class DescribeExtractingObjects
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void It_extracts_instances_that_were_placed_with_Use()
            {
                var setupInstance = Mock.Of<IService1>();
                mocker.Use(setupInstance);

                var actualInstance = mocker.Get<IService1>();
                Assert.AreSame(setupInstance, actualInstance);
            }

            [TestMethod]
            public void It_extracts_instances_that_were_setup_with_Use()
            {
                mocker.Use<IService1>(x => x.ToString() == "");
                // Assert does not throw
                mocker.GetMock<IService1>();
            }

            [TestMethod]
            public void It_creates_a_mock_if_the_oject_is_missing()
            {
                var mock = mocker.GetMock<IService1>();
                Assert.IsNotNull(mock);
            }

            [TestMethod]
            public void It_creates_a_mock_if_the_object_is_missing_using_Get()
            {
                var mock = mocker.Get<IService1>();
                Assert.IsNotNull(mock);
            }

            [TestMethod]
            public void Mocks_that_were_autogenerated_can_be_extracted()
            {
                mocker.CreateInstance<WithService>();
                var actualInstance = mocker.Get<IService2>();
                Assert.IsNotNull(actualInstance);
            }

            [TestMethod]
            public void ExtractMock_throws_ArgumentException_when_object_isnt_A_mock()
            {
                mocker.Use<IService2>(new Service2());
                Assert.ThrowsException<ArgumentException>(() => mocker.GetMock<IService2>());
            }
        }

        [TestClass]
        public class DescribeExtractingStrictObjects
        {
            private readonly AutoMocker mocker = new AutoMocker(MockBehavior.Strict);

            [TestMethod]
            public void It_creates_a_mock_as_strict_if_the_object_is_missing()
            {
                var mock = mocker.GetMock<IService1>();
                Assert.AreEqual(MockBehavior.Strict,mock.Behavior);
            }
        }


        [TestClass]
        public class DescribeCreatingSelfMocks
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void Self_mocks_are_useful_for_testing_most_of_class()
            {
                var selfMock = mocker.CreateSelfMock<InsecureAboutSelf>();
                selfMock.TellJoke();
                Assert.IsFalse(selfMock.SelfDepricated);
            }

            [TestMethod]
            public void It_can_self_mock_objects_with_constructor_arguments()
            {
                var selfMock = mocker.CreateSelfMock<WithService>();
                Assert.IsNotNull(selfMock.Service);
                Assert.IsNotNull(Mock.Get(selfMock.Service));
            }
        }

        [TestClass]
        public class DescribeVerifyAll
        {
            private readonly AutoMocker mocker = new AutoMocker();

            [TestMethod]
            public void It_calls_VerifyAll_on_all_objects_that_are_mocks()
            {
                mocker.Use<IService2>(x => x.Other == Mock.Of<IService1>());
                var _ = mocker.CreateInstance<WithService>();
                var ex = Assert.ThrowsException<MockException>(() => mocker.VerifyAll());
                Assert.IsTrue(ex.IsVerificationError);
            }

            [TestMethod]
            public void It_doesnt_call_VerifyAll_if_the_object_isnt_a_mock()
            {
                mocker.Use<IService2>(new Service2());
                mocker.CreateInstance<WithService>();
                mocker.VerifyAll();
            }
        }
    }
}
