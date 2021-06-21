using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

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
            IDisposable disposable = mocker.AsDisposable();

            disposable.Dispose();

            Assert.IsTrue(test.IsDisposed);
        }

        [TestMethod]
        public void It_disposes_using_custom_disposable_instance()
        {
            TestDisposable test = new();
            AutoMocker mocker = new();
            CustomDisposer disposer = new(mocker);
            mocker.Use(test);
            mocker.Use<IAutoMockerDisposable>(disposer);
            IDisposable disposable = mocker.AsDisposable(); 

            disposable.Dispose();

            Assert.IsTrue(disposer is CustomDisposer);
            Assert.IsTrue(test.IsDisposed);
        }

        private sealed class CustomDisposer : IAutoMockerDisposable
        {
            public CustomDisposer(AutoMocker mocker)
            {
                Mocker = mocker ?? throw new ArgumentNullException(nameof(mocker));
            }

            private bool IsDisposed { get; set; }
            private AutoMocker Mocker { get; }

            private void Dispose(bool disposing)
            {
                if (!IsDisposed)
                {
                    if (disposing)
                    {
                        // TODO: custom logic would go here
                        foreach (var disposable in Mocker.ResolvedObjects.Values.OfType<IDisposable>())
                        {
                            if (!ReferenceEquals(disposable, this))
                            {
                                disposable.Dispose();
                            }
                        }
                    }
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
