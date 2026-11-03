namespace KaedePhi.Tests.Converter.Extensions;

public sealed class NonSeekableReadStream(Stream source, Action onDispose) : Stream
{
    public override bool CanRead => source.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) => source.Read(buffer, offset, count);
    public override int Read(Span<byte> buffer) => source.Read(buffer);
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        source.ReadAsync(buffer, cancellationToken);
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        source.ReadAsync(buffer, offset, count, cancellationToken);
    public override void Flush() => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            source.Dispose();
            onDispose();
        }
        base.Dispose(disposing);
    }
}
