namespace KaedePhi.Tool.Converter;

internal sealed class ProgressWriteStream : Stream
{
    private readonly Stream _inner;
    private readonly bool _leaveOpen;
    private readonly Action<long>? _onBytesWritten;

    public ProgressWriteStream(
        Stream inner,
        bool leaveOpen = false,
        Action<long>? onBytesWritten = null
    )
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
        _leaveOpen = leaveOpen;
        _onBytesWritten = onBytesWritten;
    }

    public long BytesWritten { get; private set; }

    public override bool CanRead => false;

    public override bool CanSeek => _inner.CanSeek;

    public override bool CanWrite => _inner.CanWrite;

    public override long Length => _inner.CanSeek ? _inner.Length : BytesWritten;

    public override long Position
    {
        get => _inner.CanSeek ? _inner.Position : BytesWritten;
        set => _inner.Position = value;
    }

    public override void Flush() => _inner.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        _inner.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

    public override void SetLength(long value) => _inner.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
    {
        _inner.Write(buffer, offset, count);
        ReportBytesWritten(count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        _inner.Write(buffer);
        ReportBytesWritten(buffer.Length);
    }

    public override async Task WriteAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    )
    {
        await _inner.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        ReportBytesWritten(count);
    }

    public override async ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default
    )
    {
        await _inner.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        ReportBytesWritten(buffer.Length);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_leaveOpen)
            _inner.Dispose();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
            await _inner.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }

    private void ReportBytesWritten(int count)
    {
        BytesWritten += count;
        _onBytesWritten?.Invoke(BytesWritten);
    }
}
