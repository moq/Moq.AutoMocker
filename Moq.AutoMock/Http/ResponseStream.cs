using System.Net.Http;

namespace Moq.AutoMock.Http;

/// <summary>
/// A wrapper that maintains its own stream position independent of the underlying stream in order to keep requests
/// from interferring with one another while reusing a stream. Also prevents a disposing <see cref="StreamContent"/>
/// from closing the underlying stream. Like a real HttpContentReadStream, this stream is neither seekable nor
/// writable. Requires a seekable stream.
/// </summary>
internal class ResponseStream : Stream
{
    private readonly Stream inner;
    private long position;

    public ResponseStream(Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new ArgumentException($"The {nameof(ResponseStream)} wrapper cannot be used with a non-seekable stream.", nameof(stream));
        }

        inner = stream;
        position = inner.Position;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        lock (inner)
        {
            // Switch to wrapper's position
            long originalPosition = inner.Position;
            inner.Position = position;

            // Read normally
            int ret = inner.Read(buffer, offset, count);

            // Swap positions back
            position = inner.Position;
            inner.Position = originalPosition;

            return ret;
        }
    }

    public override bool CanRead => inner.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
    public override void Flush() { /* no op */ }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
