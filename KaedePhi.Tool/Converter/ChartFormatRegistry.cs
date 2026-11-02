using System.Collections.ObjectModel;
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
using KpcChart = KaedePhi.Core.KaedePhi.Chart;

namespace KaedePhi.Tool.Converter;

/// <summary>
/// 谱面格式注册表，集中描述各格式的导入导出能力
/// </summary>
public static class ChartFormatRegistry
{
    private static readonly object ExternalLock = new();
    private static readonly Dictionary<string, ChartFormatDescriptor> ExternalDescriptors =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly ReadOnlyDictionary<ChartType, ChartFormatDescriptor> Descriptors = new(
        new Dictionary<ChartType, ChartFormatDescriptor>
        {
            [ChartType.RePhiEdit] = new()
            {
                Type = ChartType.RePhiEdit,
                FileExtension = "json",
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
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new ConvertOption())
                    );
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },

            [ChartType.StellateRePhiEditExtended] = new()
            {
                Type = ChartType.StellateRePhiEditExtended,
                FileExtension = "json",
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
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new ConvertOption())
                    );
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },

            [ChartType.PhiEdit] = new()
            {
                Type = ChartType.PhiEdit,
                FileExtension = "pec",
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
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new IrToPhiEditConvertOptions())
                    );
                    await WriteAsync(
                        path,
                        write,
                        target.ExportAsync,
                        target.ExportToStreamAsync,
                        ct
                    );
                },
            },

            [ChartType.PhigrosV3] = new()
            {
                Type = ChartType.PhigrosV3,
                FileExtension = "json",
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
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new IrToPhigrosV3ConvertOptions())
                    );
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },

            [ChartType.PhiChain] = new()
            {
                Type = ChartType.PhiChain,
                FileExtension = "json",
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
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new IrToPhiChainConvertOptions())
                    );
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },

            [ChartType.PhiFans] = new()
            {
                Type = ChartType.PhiFans,
                FileExtension = "json",
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
                    var target = converter.FromIr(
                        chart,
                        Coerce(options, () => new IrToPhiFansConvertOptions())
                    );
                    await WriteAsync(
                        path,
                        write,
                        () => target.ExportToJsonAsync(write.Indented),
                        stream => target.ExportToJsonStreamAsync(stream, write.Indented),
                        ct
                    );
                },
            },
            [ChartType.PhigrosV1] = new() { Type = ChartType.PhigrosV1, FileExtension = "json" },
        }
    );

    /// <summary>
    /// 获取指定格式的能力描述。
    /// </summary>
    /// <param name="type">谱面格式</param>
    /// <returns>格式描述，未注册时返回 <see langword="null"/></returns>
    public static ChartFormatDescriptor? Find(ChartType type) =>
        Descriptors.GetValueOrDefault(type);

    /// <summary>
    /// 按格式标识获取内置或外部格式描述，标识不区分大小写。
    /// </summary>
    /// <param name="formatId">格式的稳定字符串标识。</param>
    /// <returns>格式描述，未注册时返回 <see langword="null"/>。</returns>
    public static ChartFormatDescriptor? Find(string formatId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        var builtIn = Descriptors.Values.FirstOrDefault(descriptor =>
            string.Equals(descriptor.FormatId, formatId, StringComparison.OrdinalIgnoreCase));
        if (builtIn is not null)
            return builtIn;
        lock (ExternalLock)
            return ExternalDescriptors.GetValueOrDefault(formatId);
    }

    /// <summary>
    /// 注册外部格式；标识不区分大小写，重复标识及内置格式名称不能被覆盖。
    /// </summary>
    /// <param name="descriptor">由 <see cref="ChartFormatDescriptor.Create"/> 创建的格式描述。</param>
    public static void Register(ChartFormatDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (descriptor.Type != (ChartType)(-1))
            throw new ArgumentException("请使用格式工厂创建外部格式描述。", nameof(descriptor));
        if (Enum.GetNames<ChartType>().Any(name =>
                string.Equals(name, descriptor.FormatId, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"格式标识已被内置格式占用：{descriptor.FormatId}");

        lock (ExternalLock)
        {
            if (!ExternalDescriptors.TryAdd(descriptor.FormatId, descriptor))
                throw new InvalidOperationException($"格式标识已注册：{descriptor.FormatId}");
        }
    }

    /// <summary>
    /// 取消注册外部格式。
    /// </summary>
    /// <param name="formatId">外部格式的稳定字符串标识。</param>
    /// <returns>是否移除了已注册格式。</returns>
    public static bool Unregister(string formatId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        lock (ExternalLock)
            return ExternalDescriptors.Remove(formatId);
    }

    /// <summary>
    /// 获取指定格式的能力描述，未注册时抛出异常。
    /// </summary>
    /// <param name="type">谱面格式</param>
    /// <returns>格式描述</returns>
    public static ChartFormatDescriptor Get(ChartType type) =>
        Find(type) ?? throw new NotSupportedException($"未注册的谱面格式：{type}");

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
    public static IReadOnlyList<ChartFormatDescriptor> Formats
    {
        get
        {
            lock (ExternalLock)
                return Descriptors.Values.Concat(ExternalDescriptors.Values).ToArray();
        }
    }

    /// <summary>
    /// 检测谱面文本格式并转换为 IR 中间格式。
    /// </summary>
    /// <param name="text">谱面原始文本</param>
    /// <param name="importOptions">导入选项，传 <see langword="null"/> 时使用格式默认值</param>
    /// <param name="log">日志回调集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>检测到的格式与转换结果</returns>
    public static async Task<(ChartType DetectedType, Ir.Chart Chart)> ImportIrAsync(
        string text,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    )
    {
        using var textReader = new StringReader(text);
        var detectedType = ChartGetType.GetType(textReader);
        var chart = await Get(detectedType).ImportIrAsync(text, importOptions, log, ct);
        return (detectedType, chart);
    }

    /// <summary>
    /// 检测谱面文本格式并转换为已弃用的 KPC 中间格式。
    /// </summary>
    /// <param name="text">谱面原始文本</param>
    /// <param name="importOptions">导入选项，传 <see langword="null"/> 时使用格式默认值</param>
    /// <param name="log">日志回调集合</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>检测到的格式与转换结果</returns>
    [Obsolete("已弃用：请迁移至 ImportIrAsync。")]
    public static async Task<(ChartType DetectedType, KpcChart Chart)> ImportAsync(
        string text,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    )
    {
        var (detectedType, chart) = await ImportIrAsync(text, importOptions, log, ct);
        return (detectedType, Compatibility.KpcCompatibilityMapper.ToKpc(chart));
    }

    /// <summary>
    /// 为转换器注入日志回调与取消令牌。
    /// </summary>
    private static TConverter Prepare<TConverter>(
        TConverter converter,
        ChartLogSink log,
        CancellationToken ct
    )
        where TConverter : ILoggable
    {
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
            if (write.DryRun)
            {
                await using var stream = Stream.Null;
                await serializeStream(stream);
            }
            else
            {
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
                        await serializeStream(stream);
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
        }
        else
        {
            var text = await serializeText();
            if (!write.DryRun)
            {
                var temporaryPath = fullPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
                Directory.CreateDirectory(
                    Path.GetDirectoryName(fullPath)
                        ?? throw new InvalidOperationException("Invalid output path")
                );
                try
                {
                    await File.WriteAllTextAsync(temporaryPath, text, ct);
                    ct.ThrowIfCancellationRequested();
                    File.Move(temporaryPath, fullPath, true);
                }
                finally
                {
                    if (File.Exists(temporaryPath))
                        File.Delete(temporaryPath);
                }
            }
        }
    }
}
