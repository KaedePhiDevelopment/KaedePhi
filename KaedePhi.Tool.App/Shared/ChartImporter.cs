using System.Text;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using Chart = KaedePhi.Core.Intermediate.Model.Chart;

namespace KaedePhi.Tool.App.Shared;

// CLI 与 GUI 共用的谱面格式检测与导入流程
internal static class ChartImporter
{
    // 检测格式后会将进度复位为不确定状态
    public static async Task<ChartFormatDescriptor> DetectFormatAsync(
        string filePath,
        IProgress<double?>? progress,
        CancellationToken ct
    )
    {
        await using var stream = OpenRead(filePath, progress);
        var format = await ChartGetType.DetectFormatAsync(stream, ct);
        progress?.Report(null);
        return format;
    }

    // preferStream 为 true 时，格式支持流式导入则优先使用；格式不支持文本导入时总是使用流式导入
    public static async Task<Chart> ImportAsync(
        string filePath,
        ChartFormatDescriptor format,
        bool preferStream,
        object? importOptions,
        ChartLogSink? log,
        IProgress<double?>? progress,
        CancellationToken ct
    )
    {
        await using var stream = OpenRead(filePath, progress);
        if (format.CanStreamImport && (preferStream || !format.CanTextImport))
            return await format.ImportStreamIrAsync(stream, importOptions, log, ct);

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096,
            leaveOpen: true
        );
        var text = await reader.ReadToEndAsync(ct);
        progress?.Report(null);
        return await format.ImportIrAsync(text, importOptions, log, ct);
    }

    private static Stream OpenRead(string filePath, IProgress<double?>? progress)
    {
        var stream = ChartFiles.OpenRead(filePath);
        return progress is null ? stream : new ProgressReadStream(stream, progress);
    }
}
