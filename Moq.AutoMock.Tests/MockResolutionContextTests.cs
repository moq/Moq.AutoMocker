using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock.Resolvers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class MockResolutionContextTests
    {
        [TestMethod]
        [DynamicData(nameof(Arguments))]
        public void It_asserts_null_dependency(AutoMocker mocker, Type type, ObjectGraphContext context)
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new MockResolutionContext(mocker, type, initialValue: null, context));
        }

        private static IEnumerable<object[]> Arguments
        {
            get
            {
                AutoMocker mocker = new();
                Type type = typeof(MockResolutionContextTests);
                ObjectGraphContext context = new(false);
                yield return new object[] { null!, type, context };
                yield return new object[] { mocker, null!, context };
                yield return new object[] { mocker, type, null! };
            }
        }
    }
}
