using System.Diagnostics.CodeAnalysis;
using KaedePhi.Core.Formats;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter;

/// <summary>
/// 单一谱面格式的导入导出能力描述。由 <see cref="ChartFormatRegistry"/> 统一注册与查询。
/// </summary>
public sealed class ChartFormatDescriptor
{
    private readonly string? _formatId;

    [SetsRequiredMembers]
    internal ChartFormatDescriptor(string formatId, string fileExtension)
    {
        _formatId = formatId;
        FileExtension = fileExtension;
    }

    /// <summary>格式的稳定字符串标识；内置格式使用枚举名称。</summary>
    public string FormatId => _formatId ?? throw new InvalidOperationException("未设置格式标识。");

    /// <summary>该格式的默认文件扩展名（不含点）。</summary>
    public required string FileExtension { get; init; }

    /// <summary>创建该格式导入选项的默认实例，无选项时返回 <see langword="null"/>。</summary>
    public Func<object?>? ImportOptionsFactory { get; init; }

    /// <summary>创建该格式导出选项的默认实例，无选项时返回 <see langword="null"/>。</summary>
    public Func<object?>? ExportOptionsFactory { get; init; }

    /// <summary>
    /// 使用稳定字符串标识创建谱面格式描述。
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
        Func<Stream, object?, ChartLogSink, CancellationToken, Task<Ir.Chart>>? streamImporter =
            null,
        Func<
            Ir.Chart,
            string,
            ChartWriteSettings,
            object?,
            ChartLogSink,
            CancellationToken,
            Task
        >? exporter = null,
        Func<object?>? importOptionsFactory = null,
        Func<object?>? exportOptionsFactory = null
    )
    {
        ValidateIdentityNames(formatId, fileExtension);
        if (importer is null && streamImporter is null && exporter is null)
            throw new ArgumentException("至少提供一种导入或导出能力。", nameof(importer));
        ValidateExtension(fileExtension);
        return new ChartFormatDescriptor(formatId, fileExtension)
        {
            Importer = importer,
            StreamImporter = streamImporter,
            Exporter = exporter,
            ImportOptionsFactory = importOptionsFactory,
            ExportOptionsFactory = exportOptionsFactory,
        };
    }

    /// <summary>
    /// 创建仅用于检测的谱面格式描述。
    /// </summary>
    /// <param name="formatId">格式的稳定唯一标识。</param>
    /// <param name="fileExtension">默认文件扩展名，不含点。</param>
    /// <returns>不支持导入和导出的格式描述。</returns>
    public static ChartFormatDescriptor CreateDetectionOnly(string formatId, string fileExtension)
    {
        ValidateIdentityNames(formatId, fileExtension);
        ValidateExtension(fileExtension);
        return new ChartFormatDescriptor(formatId, fileExtension);
    }

    private static void ValidateIdentityNames(string formatId, string fileExtension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);
        if (formatId != formatId.Trim())
            throw new ArgumentException("格式标识不能包含首尾空白。", nameof(formatId));
    }

    private static void ValidateExtension(string fileExtension)
    {
        if (fileExtension.StartsWith('.') || fileExtension.Any(c => !char.IsLetterOrDigit(c)))
            throw new ArgumentException(
                "文件扩展名只能包含字母或数字，且不含点。",
                nameof(fileExtension)
            );
    }

    /// <summary>
    /// 使用格式序列化器和转换器创建外部格式描述，导入和导出均使用流。
    /// </summary>
    /// <typeparam name="TChart">外部谱面类型。</typeparam>
    /// <typeparam name="TInOptions">导入转换选项类型。</typeparam>
    /// <typeparam name="TOutOptions">导出转换选项类型。</typeparam>
    /// <param name="formatId">格式的稳定唯一标识。</param>
    /// <param name="fileExtension">默认文件扩展名，不含点。</param>
    /// <param name="serializer">谱面序列化器；并发调用时须支持并发使用。</param>
    /// <param name="converterFactory">为每次转换创建转换器的工厂。</param>
    /// <param name="importOptionsFactory">默认导入选项工厂。</param>
    /// <param name="exportOptionsFactory">默认导出选项工厂。</param>
    /// <returns>外部格式描述。</returns>
    public static ChartFormatDescriptor Create<TChart, TInOptions, TOutOptions>(
        string formatId,
        string fileExtension,
        IChartSerializer<TChart> serializer,
        Func<IChartConverter<TChart, TInOptions, TOutOptions>> converterFactory,
        Func<TInOptions>? importOptionsFactory = null,
        Func<TOutOptions>? exportOptionsFactory = null
    )
    {
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(converterFactory);
        return Create(
            formatId,
            fileExtension,
            streamImporter: async (stream, options, log, ct) =>
            {
                var source = await serializer.ReadAsync(stream, ct);
                ct.ThrowIfCancellationRequested();
                var converter = ChartFormatRegistry.Prepare(converterFactory(), log, ct);
                return converter.ToIr(source, ResolveOptions<TInOptions>(options));
            },
            exporter: async (chart, path, write, options, log, ct) =>
            {
                var converter = ChartFormatRegistry.Prepare(converterFactory(), log, ct);
                var target = converter.FromIr(chart, ResolveOptions<TOutOptions>(options));
                ct.ThrowIfCancellationRequested();
                await ChartFormatRegistry.WriteStreamAsync(
                    path,
                    write,
                    stream => serializer.WriteAsync(target, stream, write.Indented, ct),
                    ct
                );
                ct.ThrowIfCancellationRequested();
            },
            importOptionsFactory: importOptionsFactory is null
                ? null
                : () => importOptionsFactory(),
            exportOptionsFactory: exportOptionsFactory is null ? null : () => exportOptionsFactory()
        );
    }

    private static TOptions ResolveOptions<TOptions>(object? options)
    {
        if (options is TOptions typed)
            return typed;
        if (options is null && default(TOptions) is null)
            return default!;
        throw new ArgumentException(
            $"选项类型不匹配：期望 {typeof(TOptions).Name}，实际为 {options?.GetType().Name ?? "null"}。",
            nameof(options)
        );
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

    /// <summary>该格式是否支持文本导入。</summary>
    public bool CanTextImport => Importer is not null;

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
        var chart = await Importer(
            text,
            importOptions ?? CreateDefaultImportOptions(),
            log ?? ChartLogSink.None,
            ct
        );
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
        var chart = await StreamImporter(
            stream,
            importOptions ?? CreateDefaultImportOptions(),
            log ?? ChartLogSink.None,
            ct
        );
        ct.ThrowIfCancellationRequested();
        var normalized = IrChartNormalizer.NormalizeAndValidateNoteEndBeats(chart);
        return normalized;
    }

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

    private string FormatName => FormatId;
}
