using System.Text;
using KaedePhi.Tool.Common;

namespace KaedePhi.Tool.Converter;

/// <summary>
/// 可独立管理谱面格式的注册表。
/// </summary>
public sealed class ChartFormatCatalog
{
    private readonly object _lock = new();
    private readonly Dictionary<string, ChartFormatRegistration> _registrations = new(
        StringComparer.OrdinalIgnoreCase
    );

    /// <summary>
    /// 创建独立注册表，可选择是否注册默认格式。
    /// </summary>
    /// <param name="includeBuiltInFormats">是否注册默认内置格式。</param>
    public ChartFormatCatalog(bool includeBuiltInFormats = true)
        : this(includeBuiltInFormats ? ChartFormatRegistry.DefaultRegistrations : []) { }

    internal ChartFormatCatalog(IEnumerable<ChartFormatRegistration> registrations)
    {
        foreach (var registration in registrations)
            Register(registration);
    }

    /// <summary>
    /// 注册谱面格式及其检测配置，重复标识不能被覆盖。
    /// </summary>
    /// <param name="registration">格式注册信息。</param>
    public void Register(ChartFormatRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentException.ThrowIfNullOrWhiteSpace(registration.Descriptor.FormatId);
        lock (_lock)
        {
            if (!_registrations.TryAdd(registration.Descriptor.FormatId, registration))
                throw new InvalidOperationException(
                    $"格式标识已注册：{registration.Descriptor.FormatId}"
                );
        }
    }

    /// <summary>
    /// 注册不参与自动检测的谱面格式。
    /// </summary>
    /// <param name="descriptor">格式描述。</param>
    public void Register(ChartFormatDescriptor descriptor) =>
        Register(new ChartFormatRegistration(descriptor));

    /// <summary>
    /// 注册谱面格式及其检测器。
    /// </summary>
    /// <param name="descriptor">格式描述。</param>
    /// <param name="detector">格式检测回调，不匹配时返回假；流可能不可定位，回调返回后不可继续使用。</param>
    /// <param name="priority">检测优先级，越大越优先；同级匹配多个格式时抛出异常。</param>
    public void Register(
        ChartFormatDescriptor descriptor,
        Func<Stream, CancellationToken, ValueTask<bool>> detector,
        int priority = 0
    )
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(detector);
        async ValueTask<string?> Detect(Stream stream, CancellationToken ct) =>
            await detector(stream, ct) ? descriptor.FormatId : null;
        Register(new ChartFormatRegistration(descriptor, Detect, priority));
    }

    /// <summary>
    /// 按标识取消注册谱面格式及检测器。
    /// </summary>
    /// <param name="formatId">格式标识，不区分大小写。</param>
    /// <returns>是否移除了格式。</returns>
    public bool Unregister(string formatId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        lock (_lock)
            return _registrations.Remove(formatId);
    }

    /// <summary>
    /// 按标识查询格式注册信息。
    /// </summary>
    /// <param name="formatId">格式标识，不区分大小写。</param>
    /// <returns>注册信息，未注册时返回空。</returns>
    public ChartFormatRegistration? FindRegistration(string formatId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(formatId);
        lock (_lock)
            return _registrations.GetValueOrDefault(formatId);
    }

    /// <summary>
    /// 按标识查询格式描述。
    /// </summary>
    /// <param name="formatId">格式标识，不区分大小写。</param>
    /// <returns>格式描述，未注册时返回空。</returns>
    public ChartFormatDescriptor? Find(string formatId) => FindRegistration(formatId)?.Descriptor;

    /// <summary>
    /// 按标识获取已注册的格式描述。
    /// </summary>
    /// <param name="formatId">格式标识，不区分大小写。</param>
    /// <returns>格式描述。</returns>
    public ChartFormatDescriptor Get(string formatId) =>
        Find(formatId) ?? throw new NotSupportedException($"未注册的谱面格式：{formatId}");

    /// <summary>已注册格式及检测配置的快照。</summary>
    public IReadOnlyList<ChartFormatRegistration> Registrations
    {
        get
        {
            lock (_lock)
                return _registrations.Values.ToArray();
        }
    }

    /// <summary>已注册格式描述的快照。</summary>
    public IReadOnlyList<ChartFormatDescriptor> Formats =>
        Registrations.Select(registration => registration.Descriptor).ToArray();

    /// <summary>所有支持导入的格式。</summary>
    public IEnumerable<ChartFormatDescriptor> ImportableFormats =>
        Formats.Where(format => format.CanImport);

    /// <summary>所有支持导出的格式。</summary>
    public IEnumerable<ChartFormatDescriptor> ExportableFormats =>
        Formats.Where(format => format.CanExport);

    /// <summary>
    /// 检测谱面文本的已注册格式。
    /// </summary>
    /// <param name="text">待检测的谱面文本。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述。</returns>
    public Task<ChartFormatDescriptor> DetectFormatAsync(
        string text,
        CancellationToken ct = default
    ) => ChartGetType.DetectFormatAsync(this, text, ct);

    /// <summary>
    /// 从可定位流的当前位置检测已注册格式，并在结束时恢复位置。
    /// </summary>
    /// <param name="stream">可读取、可定位的谱面输入流。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述。</returns>
    public Task<ChartFormatDescriptor> DetectFormatAsync(
        Stream stream,
        CancellationToken ct = default
    ) => ChartGetType.DetectFormatAsync(this, stream, ct);

    /// <summary>
    /// 使用流工厂检测已注册格式，支持不可定位的输入。
    /// </summary>
    /// <param name="openRead">每次返回从谱面起点开始的独立可读流，使用后关闭。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述。</returns>
    public Task<ChartFormatDescriptor> DetectFormatAsync(
        Func<Stream> openRead,
        CancellationToken ct = default
    ) => ChartGetType.DetectFormatAsync(this, openRead, ct);

    /// <summary>
    /// 检测谱面文本并转换为 IR。
    /// </summary>
    /// <param name="text">谱面原始文本。</param>
    /// <param name="importOptions">导入选项，传空时使用格式默认值。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述与 IR 谱面。</returns>
    public async Task<(ChartFormatDescriptor Format, Ir.Chart Chart)> ImportDetectedIrAsync(
        string text,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(text);
        var format = await DetectFormatAsync(text, ct);
        if (!format.CanImport)
            throw new NotSupportedException($"{format.FormatId} 不支持作为导入源。");
        if (format.Importer is not null)
            return (format, await format.ImportIrAsync(text, importOptions, log, ct));
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text), writable: false);
        return (format, await format.ImportStreamIrAsync(stream, importOptions, log, ct));
    }

    /// <summary>
    /// 从可定位流的当前位置检测格式并转换为 IR。
    /// </summary>
    /// <param name="stream">可读取、可定位的输入流；导入实现应保持其打开。</param>
    /// <param name="importOptions">导入选项，传空时使用格式默认值。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述与 IR 谱面。</returns>
    public async Task<(ChartFormatDescriptor Format, Ir.Chart Chart)> ImportDetectedIrAsync(
        Stream stream,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    )
    {
        var format = await DetectFormatAsync(stream, ct);
        return (format, await ImportStreamAsync(format, stream, importOptions, log, ct));
    }

    /// <summary>
    /// 使用流工厂检测格式并转换为 IR，支持不可定位的输入。
    /// </summary>
    /// <param name="openRead">每次返回从谱面起点开始的独立可读流，使用后关闭。</param>
    /// <param name="importOptions">导入选项，传空时使用格式默认值。</param>
    /// <param name="log">日志回调集合。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的格式描述与 IR 谱面。</returns>
    public async Task<(ChartFormatDescriptor Format, Ir.Chart Chart)> ImportDetectedIrAsync(
        Func<Stream> openRead,
        object? importOptions = null,
        ChartLogSink? log = null,
        CancellationToken ct = default
    )
    {
        var format = await DetectFormatAsync(openRead, ct);
        ct.ThrowIfCancellationRequested();
        await using var stream = ChartGetType.OpenDetectionStream(openRead);
        return (format, await ImportStreamAsync(format, stream, importOptions, log, ct));
    }

    private static async Task<Ir.Chart> ImportStreamAsync(
        ChartFormatDescriptor format,
        Stream stream,
        object? options,
        ChartLogSink? log,
        CancellationToken ct
    )
    {
        if (format.CanStreamImport)
            return await format.ImportStreamIrAsync(stream, options, log, ct);
        if (!format.CanImport)
            throw new NotSupportedException($"{format.FormatId} 不支持作为导入源。");
        using var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: true);
        var text = await reader.ReadToEndAsync(ct);
        return await format.ImportIrAsync(text, options, log, ct);
    }
}
