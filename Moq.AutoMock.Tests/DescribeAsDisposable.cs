using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class DescribeAsDisposable
    {
        [TestMethod]
        public void It_disposes_all_registered_disposable_instances()
        {
            TestDisposable test = new();
            AutoMocker mocker = new();
            mocker.Use(test);

            using (mocker.AsDisposable())
            { }

            Assert.IsTrue(test.IsDisposed);
        }

        private sealed class TestDisposable : IDisposable
        {
            public bool IsDisposed { get; set; }

            private void Dispose(bool disposing)
            {
                if (!IsDisposed)
                {
                    IsDisposed = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
