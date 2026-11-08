using System.Text;
using KaedePhi.Core.Formats;

namespace KaedePhi.Tests.Converter.Extensions;

public sealed class ExternalChartSerializer : IChartSerializer<ExternalChart>
{
    public bool FailWrite { get; set; }

    public bool? LastIndented { get; private set; }

    public async Task<ExternalChart> ReadAsync(Stream stream, CancellationToken ct = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
        if (await reader.ReadLineAsync(ct) != "KAED_TEST")
            throw new InvalidDataException("格式标记无效。");
        return new ExternalChart { Name = await reader.ReadToEndAsync(ct) };
    }

    public async Task WriteAsync(
        ExternalChart chart,
        Stream stream,
        bool indented,
        CancellationToken ct = default
    )
    {
        LastIndented = indented;
        using var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true);
        await writer.WriteAsync($"KAED_TEST\n{chart.Name}|{chart.LayerCount}".AsMemory(), ct);
        await writer.FlushAsync(ct);
        if (FailWrite)
            throw new InvalidDataException("写入失败。");
    }
}
