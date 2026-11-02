using KaedePhi.Tool.Common;
using KpcChart = KaedePhi.Core.KaedePhi.Chart;

namespace KaedePhi.Tool.Converter;

/// <summary>
/// 单一谱面格式的导入导出能力描述。由 <see cref="ChartFormatRegistry"/> 统一注册与查询。
/// </summary>
public sealed class ChartFormatDescriptor
{
    /// <summary>内置格式类型；由工厂创建的外部格式返回未定义的枚举值 -1，应使用 <see cref="FormatId"/> 标识。</summary>
    public required ChartType Type { get; init; }

    /// <summary>格式的稳定字符串标识；内置格式使用枚举名称。</summary>
    public string FormatId => ExternalFormatId ?? Type.ToString();

    private string? ExternalFormatId { get; init; }

    /// <summary>该格式的默认文件扩展名（不含点）。</summary>
    public required string FileExtension { get; init; }

    /// <summary>创建该格式导入选项的默认实例，无选项时返回 <see langword="null"/>。</summary>
    public Func<object?>? ImportOptionsFactory { get; init; }

    /// <summary>创建该格式导出选项的默认实例，无选项时返回 <see langword="null"/>。</summary>
    public Func<object?>? ExportOptionsFactory { get; init; }

    /// <summary>
    /// 创建不依赖内置 <see cref="ChartType"/> 枚举的外部格式描述。
    /// </summary>
    /// <param name="formatId">格式的稳定唯一标识。</param>
    /// <param name="fileExtension">默认文件扩展名，不含点。</param>
    /// <param name="importer">文本导入回调。</param>
    /// <param name="streamImporter">流式导入回调。</param>
    /// <param name="exporter">导出回调。</param>
    /// <param name="importOptionsFactory">导入选项工厂。</param>
    /// <param name="exportOptionsFactory">导出选项工厂。</param>
    /// <returns>外部格式描述。</returns>
    public static ChartFormatDescriptor Create(
        string formatId,
        string fileExtension,
        Func<string, object?, ChartLogSink, CancellationToken, Task<Ir.Chart>>? importer = null,
        Func<Stream, object?, ChartLogSink, CancellationToken, Task<Ir.Chart>>? streamImporter = null,
        Func<Ir.Chart, string, ChartWriteSettings, object?, ChartLogSink, CancellationToken, Task>? exporter = null,
        Func<object?>? importOptionsFactory = null,
        Func<object?>? exportOptionsFactory = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);
        if (formatId != formatId.Trim())
            throw new ArgumentException("格式标识不能包含首尾空白。", nameof(formatId));
        if (importer is null && streamImporter is null && exporter is null)
            throw new ArgumentException("至少提供一种导入或导出能力。", nameof(importer));
        if (fileExtension.StartsWith('.') || fileExtension.Any(c => !char.IsLetterOrDigit(c)))
            throw new ArgumentException("文件扩展名只能包含字母或数字，且不含点。", nameof(fileExtension));
        return new ChartFormatDescriptor
        {
            Type = (ChartType)(-1),
            ExternalFormatId = formatId,
            FileExtension = fileExtension,
            Importer = importer,
            StreamImporter = streamImporter,
            Exporter = exporter,
            ImportOptionsFactory = importOptionsFactory,
            ExportOptionsFactory = exportOptionsFactory,
        };
    }

    /// <summary>将该格式的谱面文本转换为 IR 中间格式。</summary>
    internal Func<
        string,
        object?,
        ChartLogSink,
        CancellationToken,
        Task<Ir.Chart>
    >? Importer { get; init; }

    /// <summary>将该格式的谱面流转换为 IR 中间格式。</summary>
    internal Func<
        Stream,
        object?,
        ChartLogSink,
        CancellationToken,
        Task<Ir.Chart>
    >? StreamImporter { get; init; }

    /// <summary>将 IR 中间格式导出为该格式并写入目标路径。</summary>
    internal Func<
        Ir.Chart,
        string,
        ChartWriteSettings,
        object?,
        ChartLogSink,
        CancellationToken,
        Task
    >? Exporter { get; init; }

    /// <summary>该格式是否支持作为导入源。</summary>
    public bool CanImport => Importer is not null || StreamImporter is not null;

    /// <summary>
    /// 该格式是否支持流式导入。
    /// </summary>
    public bool CanStreamImport => StreamImporter is not null;

    /// <summary>该格式是否支持作为导出目标。</summary>
    public bool CanExport => Exporter is not null;

    /// <summary>
    /// 创建该格式的默认导入选项。
    /// </summary>
    /// <returns>选项实例，该格式无导入选项时返回 <see langword="null"/></returns>
    public object? CreateDefaultImportOptions() => ImportOptionsFactory?.Invoke();

    /// <summary>
    /// 创建该格式的默认导出选项。
    /// </summary>
    /// <returns>选项实例，该格式无导出选项时返回 <see langword="null"/></returns>
    public object? CreateDefaultExportOptions() => ExportOptionsFactory?.Invoke();

    /// <summary>
    /// 将该格式的谱面文本转换为 IR 中间格式。
    /// </summary>
    /// <param name="text">谱面原始文本</param>
    /// <param name="importOptions">导入选项，传 <see langword="null"/> 时使用默认值</param>
    /// <param name="log">日志回调集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>IR 谱面</returns>
    public async Task<Ir.Chart> ImportIrAsync(
        string text,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    )
    {
        if (Importer is null)
            throw new NotSupportedException($"{FormatName} 不支持作为导入源。");
        ArgumentNullException.ThrowIfNull(text);
        ct.ThrowIfCancellationRequested();
        var chart = await Importer(text, importOptions ?? CreateDefaultImportOptions(), log ?? ChartLogSink.None, ct);
        ct.ThrowIfCancellationRequested();
        var normalized = IrChartNormalizer.NormalizeAndValidateNoteEndBeats(chart);
        return normalized;
    }

    /// <summary>
    /// 从输入流转换为 IR 谱面。
    /// </summary>
    /// <param name="stream">输入流。</param>
    /// <param name="importOptions">导入选项。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>IR 谱面。</returns>
    public async Task<Ir.Chart> ImportStreamIrAsync(
        Stream stream,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (StreamImporter is null)
            throw new NotSupportedException($"{FormatName} 不支持流式导入。");

        ct.ThrowIfCancellationRequested();
        var chart = await StreamImporter(stream, importOptions ?? CreateDefaultImportOptions(), log ?? ChartLogSink.None, ct);
        ct.ThrowIfCancellationRequested();
        var normalized = IrChartNormalizer.NormalizeAndValidateNoteEndBeats(chart);
        return normalized;
    }

    /// <summary>
    /// 将该格式的谱面文本转换为已弃用的 KPC 中间格式。
    /// </summary>
    /// <param name="text">谱面原始文本</param>
    /// <param name="importOptions">导入选项，传 <see langword="null"/> 时使用默认值</param>
    /// <param name="log">日志回调集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>KPC 谱面</returns>
    [Obsolete("已弃用：请迁移至 ImportIrAsync。")]
    public async Task<KpcChart> ImportAsync(
        string text,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) =>
        Compatibility.KpcCompatibilityMapper.ToKpc(
            await ImportIrAsync(text, importOptions, log, ct)
        );

    /// <summary>
    /// 从输入流转换为已弃用的 KPC 中间格式。
    /// </summary>
    /// <param name="stream">输入流。</param>
    /// <param name="importOptions">导入选项。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>KPC 谱面。</returns>
    [Obsolete("已弃用：请迁移至 ImportStreamIrAsync。")]
    public async Task<KpcChart> ImportStreamAsync(
        Stream stream,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) =>
        Compatibility.KpcCompatibilityMapper.ToKpc(
            await ImportStreamIrAsync(stream, importOptions, log, ct)
        );

    /// <summary>
    /// 将 IR 谱面导出为该格式并写入指定路径。
    /// </summary>
    /// <param name="chart">IR 谱面</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="writeSettings">写入方式设置</param>
    /// <param name="exportOptions">导出选项，传 <see langword="null"/> 时使用默认值</param>
    /// <param name="log">日志回调集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>导出任务</returns>
    public Task ExportAsync(
        Ir.Chart chart,
        string outputPath,
        ChartWriteSettings? writeSettings = null,
        object? exportOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    )
    {
        if (Exporter is null)
            throw new NotSupportedException($"{FormatName} 不支持作为导出目标。");
        ArgumentNullException.ThrowIfNull(chart);
        ct.ThrowIfCancellationRequested();
        var normalized = IrChartNormalizer.NormalizeAndValidateNoteEndBeats(chart);
        return Exporter(
            normalized,
            outputPath,
            writeSettings ?? new ChartWriteSettings(),
            exportOptions ?? CreateDefaultExportOptions(),
            log ?? ChartLogSink.None,
            ct
        );
    }

    /// <summary>
    /// 将已弃用的 KPC 谱面导出为该格式并写入指定路径。
    /// </summary>
    /// <param name="chart">KPC 谱面</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="writeSettings">写入方式设置</param>
    /// <param name="exportOptions">导出选项，传 <see langword="null"/> 时使用默认值</param>
    /// <param name="log">日志回调集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>导出任务</returns>
    [Obsolete("已弃用：请迁移至 ExportAsync(Ir.Chart, ...)。")]
    public Task ExportAsync(
        KpcChart chart,
        string outputPath,
        ChartWriteSettings? writeSettings = null,
        object? exportOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) =>
        ExportAsync(
            Compatibility.KpcCompatibilityMapper.ToIntermediate(chart),
            outputPath,
            writeSettings,
            exportOptions,
            log,
            ct
        );

    private string FormatName => FormatId;
}
