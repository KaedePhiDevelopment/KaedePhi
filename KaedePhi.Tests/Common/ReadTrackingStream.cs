namespace KaedePhi.Tests.Common;

internal sealed class ReadTrackingStream(byte[] bytes) : MemoryStream(bytes, writable: false)
{
    public long FarthestReadPosition { get; private set; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = base.Read(buffer, offset, count);
        FarthestReadPosition = Math.Max(FarthestReadPosition, Position);
        return read;
    }

    public override int Read(Span<byte> buffer)
    {
        var read = base.Read(buffer);
        FarthestReadPosition = Math.Max(FarthestReadPosition, Position);
        return read;
    }

    public override async Task<int> ReadAsync(
        byte[] buffer,
        int offset,
        int count,
        CancellationToken cancellationToken
    )
    {
        var read = await base.ReadAsync(buffer, offset, count, cancellationToken);
        FarthestReadPosition = Math.Max(FarthestReadPosition, Position);
        return read;
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer,
        CancellationToken cancellationToken = default
    )
    {
        var read = await base.ReadAsync(buffer, cancellationToken);
        FarthestReadPosition = Math.Max(FarthestReadPosition, Position);
        return read;
    }
}
