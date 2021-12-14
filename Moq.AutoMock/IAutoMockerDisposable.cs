using System;
using System.Linq;

namespace Moq.AutoMock;

/// <summary>
/// An interface that is used to clean up AutoMocker instances.
/// </summary>
internal interface IAutoMockerDisposable : IDisposable
{

}


internal sealed class AutoMockerDisposable : IAutoMockerDisposable
{
    private bool _isDisposed;

    private AutoMocker Mocker { get; }

    public AutoMockerDisposable(AutoMocker mocker)
    {
        Mocker = mocker ?? throw new ArgumentNullException(nameof(mocker));
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            if (disposing)
            {
                foreach (var disposable in Mocker.ResolvedObjects.Values.OfType<IDisposable>())
                {
                    if (!ReferenceEquals(disposable, this))
                    {
                        disposable.Dispose();
                    }
                }
            }

        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
