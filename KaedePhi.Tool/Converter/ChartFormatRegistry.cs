using System.Text;
using KaedePhi.Core.Formats.PhiChain.v6.Serialization;
using KaedePhi.Core.Formats.PhiEdit.Serialization;
using KaedePhi.Core.Formats.PhiFans.Serialization;
using KaedePhi.Core.Formats.Phigros.v3.Serialization;
using KaedePhi.Core.Formats.RePhiEdit.Serialization;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter.PhiChain;
using KaedePhi.Tool.Converter.PhiChain.Model;
using KaedePhi.Tool.Converter.PhiEdit;
using KaedePhi.Tool.Converter.PhiEdit.Model;
using KaedePhi.Tool.Converter.PhiFans;
using KaedePhi.Tool.Converter.PhiFans.Model;
using KaedePhi.Tool.Converter.Phigros.v3;
using KaedePhi.Tool.Converter.Phigros.v3.Model;
using KaedePhi.Tool.Converter.RePhiEdit;
using KaedePhi.Tool.Converter.RePhiEdit.Model;
using KaedePhi.Tool.Converter.StellateRePhiEditExtended;

namespace KaedePhi.Tool.Converter;

/// <summary>
/// 谱面格式注册表，集中描述各格式的导入导出能力
/// </summary>
public static class ChartFormatRegistry
{
    private static readonly Func<Stream, CancellationToken, ValueTask<string?>> DefaultDetector =
        ChartGetType.DetectBuiltInFormatAsync;

    internal static IReadOnlyList<ChartFormatRegistration> DefaultRegistrations { get; } =
    [
        new(
            new ChartFormatDescriptor(ChartFormatIds.RePhiEdit, "json")
            {
                ExportOptionsFactory = () => new ConvertOption(),
                Importer = async (text, _, log, ct) =>
                {
                    var converter = Prepare(new RePhiEditConverter(), log, ct);
                    var source = await RpeSerialization.LoadFromJsonAsync(text);
                    return converter.ToIr(source, null);
                },
                StreamImporter = async (stream, _, log, ct) =>
                {
                    var converter = Prepare(new RePhiEditConverter(), log, ct);
                    var source = await RpeSerialization.LoadFromStreamAsync(stream);
                    return converter.ToIr(source, null);
                },
                Exporter = async (chart, path, write, options, log, ct) =>
                {
                    var converter = Prepare(new RePhiEditConverter(), log, ct);
                    write.Progress?.Report(new ToolProgress(0.1));
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new ConvertOption())
                    );
                    write.Progress?.Report(new ToolProgress(0.55));
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },
            DefaultDetector
        ),
        new(
            new ChartFormatDescriptor(ChartFormatIds.StellateRePhiEditExtended, "json")
            {
                ExportOptionsFactory = () => new ConvertOption(),
                Importer = async (text, _, log, ct) =>
                {
                    var converter = Prepare(new StellateRePhiEditExtendedConverter(), log, ct);
                    var source = await RpeSerialization.LoadFromJsonAsync(text);
                    return converter.ToIr(source);
                },
                StreamImporter = async (stream, _, log, ct) =>
                {
                    var converter = Prepare(new StellateRePhiEditExtendedConverter(), log, ct);
                    var source = await RpeSerialization.LoadFromStreamAsync(stream);
                    return converter.ToIr(source);
                },
                Exporter = async (chart, path, write, options, log, ct) =>
                {
                    var converter = Prepare(new StellateRePhiEditExtendedConverter(), log, ct);
                    write.Progress?.Report(new ToolProgress(0.1));
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new ConvertOption())
                    );
                    write.Progress?.Report(new ToolProgress(0.55));
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },
            DefaultDetector
        ),
        new(
            new ChartFormatDescriptor(ChartFormatIds.PhiEdit, "pec")
            {
                ImportOptionsFactory = () => new PhiEditToIrConvertOptions(),
                ExportOptionsFactory = () => new IrToPhiEditConvertOptions(),
                Importer = async (text, options, log, ct) =>
                {
                    var converter = Prepare(new PhiEditConverter(), log, ct);
                    var source = await PeSerialization.LoadAsync(text);
                    return converter.ToIr(
                        source,
                        Coerce(options, () => new PhiEditToIrConvertOptions())
                    );
                },
                StreamImporter = async (stream, options, log, ct) =>
                {
                    var converter = Prepare(new PhiEditConverter(), log, ct);
                    var source = await PeSerialization.LoadStreamAsync(stream);
                    return converter.ToIr(
                        source,
                        Coerce(options, () => new PhiEditToIrConvertOptions())
                    );
                },
                Exporter = async (chart, path, write, options, log, ct) =>
                {
                    var converter = Prepare(new PhiEditConverter(), log, ct);
                    write.Progress?.Report(new ToolProgress(0.1));
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new IrToPhiEditConvertOptions())
                    );
                    write.Progress?.Report(new ToolProgress(0.55));
                    await WriteAsync(
                        path,
                        write,
                        target.ExportAsync,
                        target.ExportToStreamAsync,
                        ct
                    );
                },
            },
            DefaultDetector
        ),
        new(
            new ChartFormatDescriptor(ChartFormatIds.PhigrosV3, "json")
            {
                ExportOptionsFactory = () => new IrToPhigrosV3ConvertOptions(),
                Importer = async (text, _, log, ct) =>
                {
                    var converter = Prepare(new PhigrosV3Converter(), log, ct);
                    var source = await PhigrosSerialization.LoadFromJsonAsync(text);
                    return converter.ToIr(source, null);
                },
                StreamImporter = async (stream, _, log, ct) =>
                {
                    var converter = Prepare(new PhigrosV3Converter(), log, ct);
                    var source = await PhigrosSerialization.LoadFromStreamAsync(stream);
                    return converter.ToIr(source, null);
                },
                Exporter = async (chart, path, write, options, log, ct) =>
                {
                    var converter = Prepare(new PhigrosV3Converter(), log, ct);
                    write.Progress?.Report(new ToolProgress(0.1));
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new IrToPhigrosV3ConvertOptions())
                    );
                    write.Progress?.Report(new ToolProgress(0.55));
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },
            DefaultDetector
        ),
        new(
            new ChartFormatDescriptor(ChartFormatIds.PhiChain, "json")
            {
                ImportOptionsFactory = () => new PhiChainToIrConvertOptions(),
                ExportOptionsFactory = () => new IrToPhiChainConvertOptions(),
                Importer = async (text, options, log, ct) =>
                {
                    var converter = Prepare(new PhiChainConverter(), log, ct);
                    var source = await PhichainSerialization.LoadFromJsonAsync(text);
                    return converter.ToIr(
                        source,
                        Coerce(options, () => new PhiChainToIrConvertOptions())
                    );
                },
                StreamImporter = async (stream, options, log, ct) =>
                {
                    var converter = Prepare(new PhiChainConverter(), log, ct);
                    var source = await PhichainSerialization.LoadFromJsonStreamAsync(stream);
                    return converter.ToIr(
                        source,
                        Coerce(options, () => new PhiChainToIrConvertOptions())
                    );
                },
                Exporter = async (chart, path, write, options, log, ct) =>
                {
                    var converter = Prepare(new PhiChainConverter(), log, ct);
                    write.Progress?.Report(new ToolProgress(0.1));
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new IrToPhiChainConvertOptions())
                    );
                    write.Progress?.Report(new ToolProgress(0.55));
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },
            DefaultDetector
        ),
        new(
            new ChartFormatDescriptor(ChartFormatIds.PhiFans, "json")
            {
                ExportOptionsFactory = () => new IrToPhiFansConvertOptions(),
                Importer = async (text, _, log, ct) =>
                {
                    var converter = Prepare(new PhiFansConverter(), log, ct);
                    var source = await PhiFansSerialization.LoadFromJsonAsync(text);
                    return converter.ToIr(source, null);
                },
                StreamImporter = async (stream, _, log, ct) =>
                {
                    var converter = Prepare(new PhiFansConverter(), log, ct);
                    var source = await PhiFansSerialization.LoadFromStreamAsync(stream);
                    return converter.ToIr(source, null);
                },
                Exporter = async (chart, path, write, options, log, ct) =>
                {
                    var converter = Prepare(new PhiFansConverter(), log, ct);
                    write.Progress?.Report(new ToolProgress(0.1));
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new IrToPhiFansConvertOptions())
                    );
                    write.Progress?.Report(new ToolProgress(0.55));
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },
            DefaultDetector
        ),
        new(new ChartFormatDescriptor(ChartFormatIds.PhigrosV1, "json"), DefaultDetector),
    ];

    /// <summary>默认谱面格式注册表。</summary>
    public static ChartFormatCatalog Default { get; } = new(DefaultRegistrations);

    /// <summary>
    /// 按格式标识获取内置或外部格式描述，标识不区分大小写。
    /// </summary>
    /// <param name="formatId">格式的稳定字符串标识。</param>
    /// <returns>格式描述，未注册时返回 <see langword="null"/>。</returns>
    public static ChartFormatDescriptor? Find(string formatId) => Default.Find(formatId);

    /// <summary>
    /// 注册谱面格式；标识不区分大小写，重复标识不能被覆盖。
    /// </summary>
    /// <param name="descriptor">格式描述。</param>
    public static void Register(ChartFormatDescriptor descriptor) => Default.Register(descriptor);

    /// <summary>
    /// 注册谱面格式及其检测配置。
    /// </summary>
    /// <param name="registration">格式注册信息。</param>
    public static void RegisterFormat(ChartFormatRegistration registration) =>
        Default.Register(registration);

    /// <summary>
    /// 注册谱面格式及其检测器；检测器接收独立的可读流，匹配时返回真。
    /// </summary>
    /// <param name="descriptor">格式描述。</param>
    /// <param name="detector">格式检测回调，不匹配时返回假；流可能不可定位，回调返回后不可继续使用。</param>
    /// <param name="priority">检测优先级，内置格式的优先级为零；同级匹配多个格式时抛出异常。</param>
    public static void Register(
        ChartFormatDescriptor descriptor,
        Func<Stream, CancellationToken, ValueTask<bool>> detector,
        int priority = 0
    ) => Default.Register(descriptor, detector, priority);

    /// <summary>
    /// 按标识取消注册默认注册表中的谱面格式，包括内置格式。
    /// </summary>
    /// <param name="formatId">格式的稳定字符串标识。</param>
    /// <returns>是否移除了已注册格式。</returns>
    public static bool UnregisterFormat(string formatId) => Default.Unregister(formatId);

    /// <summary>
    /// 按格式标识获取内置或外部格式描述，未注册时抛出异常。
    /// </summary>
    /// <param name="formatId">格式的稳定字符串标识。</param>
    /// <returns>格式描述。</returns>
    public static ChartFormatDescriptor Get(string formatId) =>
        Find(formatId) ?? throw new NotSupportedException($"未注册的谱面格式：{formatId}");

    /// <summary>
    /// 将外部格式文本转换为 IR 中间格式。
    /// </summary>
    /// <param name="formatId">外部格式的稳定字符串标识。</param>
    /// <param name="text">谱面原始文本。</param>
    /// <param name="importOptions">导入选项。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>IR 谱面。</returns>
    public static Task<Ir.Chart> ImportExternalIrAsync(
        string formatId,
        string text,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) => Get(formatId).ImportIrAsync(text, importOptions, log, ct);

    /// <summary>
    /// 从流式输入转换外部格式为 IR 中间格式。
    /// </summary>
    /// <param name="formatId">外部格式的稳定字符串标识。</param>
    /// <param name="stream">谱面输入流。</param>
    /// <param name="importOptions">导入选项。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>IR 谱面。</returns>
    public static Task<Ir.Chart> ImportExternalStreamIrAsync(
        string formatId,
        Stream stream,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) => Get(formatId).ImportStreamIrAsync(stream, importOptions, log, ct);

    /// <summary>
    /// 将 IR 谱面导出为外部格式。
    /// </summary>
    /// <param name="formatId">外部格式的稳定字符串标识。</param>
    /// <param name="chart">IR 谱面。</param>
    /// <param name="outputPath">输出文件路径。</param>
    /// <param name="writeSettings">写入设置。</param>
    /// <param name="exportOptions">导出选项。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>导出任务。</returns>
    public static Task ExportExternalAsync(
        string formatId,
        Ir.Chart chart,
        string outputPath,
        ChartWriteSettings? writeSettings = null,
        object? exportOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) => Get(formatId).ExportAsync(chart, outputPath, writeSettings, exportOptions, log, ct);

    /// <summary>
    /// 所有支持导入的格式。
    /// </summary>
    public static IEnumerable<ChartFormatDescriptor> ImportableFormats =>
        Formats.Where(d => d.CanImport);

    /// <summary>
    /// 所有支持导出的格式。
    /// </summary>
    public static IEnumerable<ChartFormatDescriptor> ExportableFormats =>
        Formats.Where(d => d.CanExport);

    /// <summary>
    /// 获取内置与外部格式的快照。
    /// </summary>
    public static IReadOnlyList<ChartFormatDescriptor> Formats => Default.Formats;

    /// <summary>
    /// 检测谱面文本的内置或外部格式并转换为 IR。
    /// </summary>
    /// <param name="text">谱面原始文本。</param>
    /// <param name="importOptions">导入选项，传空时使用格式默认值。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述与 IR 谱面。</returns>
    public static Task<(ChartFormatDescriptor Format, Ir.Chart Chart)> ImportDetectedIrAsync(
        string text,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) => Default.ImportDetectedIrAsync(text, importOptions, log, ct);

    /// <summary>
    /// 从可定位流的当前位置检测格式并转换为 IR；检测后从原位置导入。
    /// </summary>
    /// <param name="stream">可读取、可定位的谱面输入流；导入实现应保持其打开。</param>
    /// <param name="importOptions">导入选项，传空时使用格式默认值。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述与 IR 谱面。</returns>
    public static Task<(ChartFormatDescriptor Format, Ir.Chart Chart)> ImportDetectedIrAsync(
        Stream stream,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) => Default.ImportDetectedIrAsync(stream, importOptions, log, ct);

    /// <summary>
    /// 使用流工厂检测格式并转换为 IR，支持不可定位的输入。
    /// </summary>
    /// <param name="openRead">每次调用均返回从谱面起点开始的独立可读流；使用后关闭该流。</param>
    /// <param name="importOptions">导入选项，传空时使用格式默认值。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述与 IR 谱面。</returns>
    public static Task<(ChartFormatDescriptor Format, Ir.Chart Chart)> ImportDetectedIrAsync(
        Func<Stream> openRead,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    ) => Default.ImportDetectedIrAsync(openRead, importOptions, log, ct);

    /// <summary>
    /// 为转换器注入日志回调与取消令牌。
    /// </summary>
    internal static TConverter Prepare<TConverter>(
        TConverter converter,
        ChartLogSink log,
        CancellationToken ct
    )
        where TConverter : ILoggable
    {
        ArgumentNullException.ThrowIfNull(converter);
        log.AttachTo(converter);
        if (converter is ICancellableChartConverter cancellable)
            cancellable.SetCancellationToken(ct);
        return converter;
    }

    /// <summary>
    /// 将弱类型选项转换为目标类型，为空时回退到默认值，类型不匹配时显式失败。
    /// </summary>
    private static TOptions Coerce<TOptions>(object? options, Func<TOptions> fallback)
        where TOptions : class
    {
        if (options is null)
            return fallback();
        if (options is not TOptions typed)
            throw new ArgumentException(
                $"选项类型不匹配：期望 {typeof(TOptions).Name}，实际为 {options.GetType().Name}。",
                nameof(options)
            );
        return typed;
    }

    /// <summary>
    /// 按写入设置选择整体写入或流式写入。
    /// </summary>
    private static async Task WriteAsync(
        string path,
        ChartWriteSettings write,
        Func<Task<string>> serializeText,
        Func<Stream, Task> serializeStream,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();
        var fullPath = Path.GetFullPath(path);
        if (write.UseStream)
        {
            await WriteStreamAsync(fullPath, write, serializeStream, ct);
        }
        else
        {
            write.Progress?.Report(new ToolProgress(0.57));
            var text = await serializeText();
            if (!write.DryRun)
                await WriteTextAsync(fullPath, text, write, ct);
            else
                write.Progress?.Report(new ToolProgress(0.99));
        }
    }

    private static async Task WriteTextAsync(
        string path,
        string text,
        ChartWriteSettings write,
        CancellationToken ct
    )
    {
        var temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        Directory.CreateDirectory(
            Path.GetDirectoryName(path)
                ?? throw new InvalidOperationException("Invalid output path")
        );

        try
        {
            if (write.Progress is null)
            {
                await File.WriteAllTextAsync(temporaryPath, text, ct);
            }
            else
            {
                var totalBytes = Encoding.UTF8.GetByteCount(text);
                var reportProgress = CreateWriteProgressReporter(write, totalBytes);
                await using (
                    var stream = new FileStream(
                        temporaryPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None,
                        4096,
                        useAsync: true
                    )
                )
                await using (
                    var progressStream = new ProgressWriteStream(
                        stream,
                        leaveOpen: true,
                        onBytesWritten: reportProgress
                    )
                )
                {
                    await using var writer = new StreamWriter(
                        progressStream,
                        new UTF8Encoding(false),
                        4096,
                        leaveOpen: true
                    );

                    const int chunkSize = 4096;
                    for (var offset = 0; offset < text.Length; offset += chunkSize)
                    {
                        ct.ThrowIfCancellationRequested();
                        var count = Math.Min(chunkSize, text.Length - offset);
                        await writer.WriteAsync(text.AsMemory(offset, count), ct);
                        await writer.FlushAsync(ct);
                    }

                    if (totalBytes == 0)
                        reportProgress(0);
                    await stream.FlushAsync(ct);
                }
            }

            ct.ThrowIfCancellationRequested();
            File.Move(temporaryPath, path, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    internal static async Task WriteStreamAsync(
        string path,
        ChartWriteSettings write,
        Func<Stream, Task> serializeStream,
        CancellationToken ct
    )
    {
        ct.ThrowIfCancellationRequested();
        var fullPath = Path.GetFullPath(path);
        if (write.DryRun)
        {
            write.Progress?.Report(new ToolProgress(0.57));
            await serializeStream(Stream.Null);
            write.Progress?.Report(new ToolProgress(0.99));
            return;
        }

        long totalBytes = 0;
        if (write.Progress is not null)
        {
            write.Progress.Report(new ToolProgress(0.57));
            await using var counter = new ProgressWriteStream(Stream.Null, leaveOpen: true);
            await serializeStream(counter);
            totalBytes = counter.BytesWritten;
            write.Progress.Report(new ToolProgress(0.6));
        }

        var temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        Directory.CreateDirectory(
            Path.GetDirectoryName(fullPath)
                ?? throw new InvalidOperationException("Invalid output path")
        );
        try
        {
            await using (
                var stream = new FileStream(
                    temporaryPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    useAsync: true
                )
            )
            {
                if (write.Progress is null)
                {
                    await serializeStream(stream);
                }
                else
                {
                    await using var progressStream = new ProgressWriteStream(
                        stream,
                        leaveOpen: true,
                        CreateWriteProgressReporter(write, totalBytes)
                    );
                    await serializeStream(progressStream);
                    if (totalBytes == 0)
                        write.Progress.Report(new ToolProgress(0.99));
                }
                await stream.FlushAsync(ct);
            }
            ct.ThrowIfCancellationRequested();
            File.Move(temporaryPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static Action<long> CreateWriteProgressReporter(
        ChartWriteSettings write,
        long totalBytes
    )
    {
        var lastReportedProgress = 0.6;
        return bytesWritten =>
        {
            if (write.Progress is null)
                return;

            var ratio = totalBytes <= 0 ? 1 : Math.Clamp((double)bytesWritten / totalBytes, 0, 1);
            var progress = 0.6 + ratio * 0.39;
            if (progress - lastReportedProgress < 0.01 && ratio < 1)
                return;

            lastReportedProgress = progress;
            write.Progress.Report(new ToolProgress(progress));
        };
    }
}
