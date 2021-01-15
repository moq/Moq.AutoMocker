﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Tests.Util;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeGetMock
    {
        [TestMethod]
        public void It_creates_mock_from_interface_from_generic_type_parameter()
        {
            var mocker = new AutoMocker();
            Mock<IService1> mock = mocker.GetMock<IService1>();
            Assert.IsNotNull(mock);
        }

        [TestMethod]
        public void It_creates_mock_from_interface_from_type_parameter()
        {
            var mocker = new AutoMocker();
            Mock<IService1>? mock = mocker.GetMock(typeof(IService1)) as Mock<IService1>;
            Assert.IsNotNull(mock);
        }

        [TestMethod]
        public void It_creates_mock_from_class_from_generic_type_parameter()
        {
            var mocker = new AutoMocker();
            Mock<Empty>? mock = mocker.GetMock<Empty>();
            Assert.IsNotNull(mock);
        }

        [TestMethod]
        public void It_creates_mock_from_class_from_type_parameter()
        {
            var mocker = new AutoMocker();
            Mock<Empty>? mock = mocker.GetMock(typeof(Empty)) as Mock<Empty>;
            Assert.IsNotNull(mock);
        }


        [TestMethod]
        public void It_fails_mocking_abstract_with_protected_ctor()
        {
            var mocker = new AutoMocker();
            Assert.ThrowsException<ArgumentException>(() => mocker.GetMock<ProtectedConstructor1>());
        }

        [TestMethod]
        public void It_allows_protected_abstract_mock_when_overriden()
        {
            var mocker = new AutoMocker();
            var mock = mocker.GetMock<ProtectedConstructor1>(enablePrivate: true);
            Assert.IsNotNull(mock);
            Assert.IsInstanceOfType(mock.Object, typeof(ProtectedConstructor1));
        }

        [TestMethod]
        public void It_fails_getting_mocked_object_with_protected_ctor()
        {
            var mocker = new AutoMocker();
            Assert.ThrowsException<ArgumentException>(() => mocker.Get<ProtectedConstructor1>());
        }

        [TestMethod]
        public void It_allows_getting_mocked_object_when_overriden()
        {
            var mocker = new AutoMocker();
            var @protected = mocker.Get<ProtectedConstructor1>(enablePrivate: true);
            Assert.IsNotNull(@protected);
            Assert.IsInstanceOfType(@protected, typeof(ProtectedConstructor1));
        }
    }
}