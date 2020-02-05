using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeSetups
    {
        /// <summary>
        /// Some people find this more comfortable than the Mock.Of() style
        /// </summary>
        [TestMethod]
        public void You_can_setup_a_mock_using_the_classic_Setup_style()
        {
            var mocker = new AutoMocker();
            mocker.Setup<IService2, IService1?>(x => x.Other).Returns(Mock.Of<IService1>());
            var mock = mocker.Get<IService2>();
            Assert.IsNotNull(mock);
            Assert.IsNotNull(mock!.Other);
        }

        [TestMethod]
        public void You_can_do_multiple_setups_on_a_single_interface()
        {
            var mocker = new AutoMocker();
            mocker.Setup<IService2, IService1?>(x => x.Other).Returns(Mock.Of<IService1>());
            mocker.Setup<IService2, string?>(x => x.Name).Returns("pure awesomeness");
            var mock = mocker.Get<IService2>();
            Assert.AreEqual("pure awesomeness", mock!.Name);
            Assert.IsNotNull(mock.Other);
        }

        [TestMethod]
        public void You_can_setup_a_void_void()
        {
            var x = 0;
            var mocker = new AutoMocker();
            mocker.Setup<IService1>(_ => _.Void()).Callback(() => x++);
            mocker.Get<IService1>()!.Void();
            Assert.AreEqual(1, x);
        }

        [TestMethod]
        public void You_can_setup_a_method_that_returns_a_value_type()
        {
            var mocker = new AutoMocker();
            mocker.Setup<IServiceWithPrimitives, long>(s => s.ReturnsALong()).Returns(100L);

            var mock = mocker.Get<IServiceWithPrimitives>();
            Assert.AreEqual(100L, mock!.ReturnsALong());
        }

        [TestMethod]
        public void You_can_setup_a_method_that_returns_a_reference_type_via_a_lambda_without_specifying_return_type()
        {
            var mocker = new AutoMocker();

            //a method with parameters
            mocker.Setup<IServiceWithPrimitives, string>(s => s.ReturnsAReferenceWithParameter(It.IsAny<string>()))
                    .Returns<string>(s => s += "2");

            var mock = mocker.Get<IServiceWithPrimitives>();
            Assert.AreEqual("blah2", mock!.ReturnsAReferenceWithParameter("blah"));
        }

        [TestMethod]
        public void You_can_setup_a_method_with_a_static_that_returns_a_reference_type_via_a_lambda_without_specifying_return_type()
        {
            var mocker = new AutoMocker();

            //a method with parameters
            mocker.Setup<IService4, string>(s => s.MainMethodName(WithStatic.Get()))
                    .Returns<string>(s => s + "2");

            var mock = mocker.Get<IService4>();
            Assert.AreEqual("2", mock!.MainMethodName(WithStatic.Get()));
        }

        [TestMethod]
        public void You_can_set_up_all_properties_with_one_line()
        {
            var mocker = new AutoMocker();
            mocker.SetupAllProperties<IService5>();

            var mock = mocker.Get<IService5>();

            mock!.Name = "aname";

            Assert.AreEqual("aname", mock.Name);
        }

        [TestMethod]
        public void You_can_setup_a_method_that_returns_diffrent_result_in_sequence()
        {
            var mocker = new AutoMocker();
            mocker.SetupSequence<IService4, string>(p => p.MainMethodName(It.IsAny<string>()))
                .Returns("t1")
                .Returns("t2");

            var mock = mocker.Get<IService4>()!;

            Assert.AreEqual("t1", mock.MainMethodName("any"));
            Assert.AreEqual("t2", mock.MainMethodName("any"));
        }
    }
}
