namespace KaedePhi.Tool.Common;

internal sealed class ChartProbeStream(Stream source, long origin) : Stream
{
    public override bool CanRead => source.CanRead;
    public override bool CanSeek => source.CanSeek;
    public override bool CanWrite => false;
    public override long Length => source.Length - origin;
    public override long Position
    {
        get => source.Position - origin;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            source.Position = checked(origin + value);
        }
    }

    public override void Flush() { }
    public override int Read(byte[] buffer, int offset, int count) => source.Read(buffer, offset, count);
    public override int Read(Span<byte> buffer) => source.Read(buffer);
    public override Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken
    ) => source.ReadAsync(buffer, offset, count, cancellationToken);
    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default
    ) => source.ReadAsync(buffer, cancellationToken);

    public override long Seek(long offset, SeekOrigin seekOrigin)
    {
        var position = seekOrigin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => checked(Position + offset),
            SeekOrigin.End => checked(Length + offset),
            _ => throw new ArgumentOutOfRangeException(nameof(seekOrigin)),
        };
        if (position < 0)
            throw new IOException("不能定位到谱面输入流的起始位置之前。");
        Position = position;
        return position;
    }

    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
