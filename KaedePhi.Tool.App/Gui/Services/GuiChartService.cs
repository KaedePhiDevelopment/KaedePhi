using KaedePhi.Tool.App.Shared;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Converter;
using KaedePhi.Tool.Render.Intermediate;
using Serilog;
using static KaedePhi.Tool.Localization.GuiLocalizationString;
using Chart = KaedePhi.Core.Intermediate.Model.Chart;

namespace KaedePhi.Tool.App.Gui.Services;

public sealed class GuiChartService
{
    private readonly ILogger _log;
    private string? _detectedFilePath;
    private ChartFormatDescriptor? _detectedFormat;

    public GuiChartService(LogService logService)
    {
        _log = logService.ForContext<GuiChartService>();
    }

    #region 当前谱面状态

    /// <summary>
    /// 当前加载的 KPC 图表（内存中）
    /// </summary>
    public Chart? CurrentChart { get; private set; }

    /// <summary>
    /// 源文件的格式类型
    /// </summary>
    public string SourceFormat { get; private set; } = string.Empty;

    /// <summary>
    /// 源文件路径
    /// </summary>
    public string? SourceFilePath { get; private set; }

    /// <summary>
    /// 是否已加载图表
    /// </summary>
    public bool IsLoaded => CurrentChart != null;

    /// <summary>
    /// 导入的源谱面是否含有行为暂不明确的内容
    /// </summary>
    public bool HasUnclearContent { get; private set; }

    #endregion

    #region 导入与导出

    /// <summary>
    /// 异步检测文件格式，避免在界面线程同步读取大型文件。
    /// </summary>
    /// <param name="filePath">输入文件路径。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>检测到的谱面格式。</returns>
    public Task<ChartFormatDescriptor> DetectChartFormatAsync(
        string filePath,
        CancellationToken ct = default
    ) => DetectChartFormatAsync(filePath, ct, null);

    internal async Task<ChartFormatDescriptor> DetectChartFormatAsync(
        string filePath,
        CancellationToken ct,
        IProgress<double?>? progress
    )
    {
        InputFileValidator.Validate(filePath);
        var format = await ChartImporter.DetectFormatAsync(filePath, progress, ct);
        _detectedFilePath = filePath;
        _detectedFormat = format;
        _log.Information(log_step_detected, format.FormatId);
        return format;
    }

    /// <summary>
    /// 从文件加载图表并转换为 KPC 格式存储在内存中
    /// </summary>
    public Task LoadChartAsync(
        string filePath,
        bool stream,
        CancellationToken ct,
        object? importOptions = null
    ) => LoadChartAsync(filePath, stream, ct, importOptions, null);

    internal async Task LoadChartAsync(
        string filePath,
        bool stream,
        CancellationToken ct,
        object? importOptions,
        IProgress<double?>? progress
    )
    {
        _log.Information(log_file_selected, filePath, stream);

        InputFileValidator.Validate(filePath);
        // 复用刚完成的格式检测结果，避免重复读取文件
        var hasDetectedFormat = _detectedFilePath == filePath && _detectedFormat is not null;
        var descriptor = hasDetectedFormat
            ? _detectedFormat!
            : await ChartImporter.DetectFormatAsync(filePath, null, ct);

        _detectedFilePath = null;
        _detectedFormat = null;
        if (!hasDetectedFormat)
            _log.Information(log_step_detected, descriptor.FormatId);

        if (!descriptor.CanImport)
            throw new NotSupportedException($"{descriptor.FormatId} 不支持作为导入源。");

        var kpcChart = await ChartImporter.ImportAsync(
            filePath,
            descriptor,
            stream,
            importOptions,
            CreateLogSink(),
            progress,
            ct
        );

        CurrentChart = kpcChart;
        SourceFormat = descriptor.FormatId;
        SourceFilePath = filePath;
        HasUnclearContent =
            descriptor.FormatId == ChartFormatIds.PhigrosV3 && kpcChart.BlockAreaList.Count > 0;
        progress?.Report(1.0);
    }

    /// <summary>
    /// 将当前 KPC 图表导出到指定格式和路径
    /// </summary>
    public async Task ExportChartAsync(
        string targetFormatId,
        string outputPath,
        bool stream,
        bool indented,
        object? exportOptions = null,
        CancellationToken ct = default
    )
    {
        if (CurrentChart == null)
            throw new InvalidOperationException("No chart loaded");

        _log.Information(log_exporting_to, outputPath, targetFormatId);
        await ChartFormatRegistry
            .Get(targetFormatId)
            .ExportAsync(
                CurrentChart,
                outputPath,
                new ChartWriteSettings { UseStream = stream, Indented = indented },
                exportOptions,
                CreateLogSink(),
                ct
            );
        _log.Information(log_export_done);
    }

    /// <summary>
    /// 清除当前加载的图表
    /// </summary>
    public void Clear()
    {
        CurrentChart = null;
        SourceFormat = string.Empty;
        SourceFilePath = null;
        HasUnclearContent = false;
        _detectedFilePath = null;
        _detectedFormat = null;
    }

    /// <summary>
    /// 将处理并校验通过的谱面副本提交为当前谱面。
    /// </summary>
    /// <param name="chart">已处理且校验通过的谱面副本。</param>
    public void CommitChart(Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        CurrentChart = chart;
    }

    #endregion

    #region 工具运行

    public void RunFatherUnbind(
        Chart chart,
        double precision,
        double tolerance,
        bool classic,
        bool disableCompress,
        double mergeTolerance,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        _log.Information(log_running_tool, tool_unbind_name);
        ChartProcessor.UnbindFather(
            chart,
            precision,
            tolerance,
            classic,
            disableCompress,
            mergeTolerance,
            CreateLogSink(),
            progress,
            ct
        );
    }

    public void RunLayerMerge(
        Chart chart,
        double precision,
        double tolerance,
        bool classic,
        bool disableCompress,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        _log.Information(log_running_tool, tool_layermerge_name);
        ChartProcessor.LayerMerge(
            chart,
            precision,
            tolerance,
            classic,
            disableCompress,
            CreateLogSink(),
            progress,
            ct
        );
    }

    public void RunCutEvent(
        Chart chart,
        double precision,
        double tolerance,
        bool disableCompress,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        _log.Information(log_running_tool, tool_cut_name);
        ChartProcessor.CutEvent(
            chart,
            precision,
            tolerance,
            disableCompress,
            CreateLogSink(),
            progress,
            ct
        );
    }

    public void RunFitEvent(
        Chart chart,
        double tolerance,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        _log.Information(log_running_tool, tool_fit_name);
        ChartProcessor.FitEvent(chart, tolerance, CreateLogSink(), progress, ct);
    }

    public IReadOnlyList<string> RunRender(
        Chart chart,
        string outputDir,
        IrRenderOptions options,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        _log.Information(log_running_tool, tool_render_name);

        // 输出目录为空时回退到导入谱面同级目录的 RenderLayer 文件夹
        if (string.IsNullOrWhiteSpace(outputDir))
            outputDir = Path.Combine(Path.GetDirectoryName(SourceFilePath) ?? ".", "RenderLayer");

        Directory.CreateDirectory(outputDir);
        return ChartProcessor.Render(
            chart,
            outputDir,
            options,
            log: CreateLogSink(),
            progress: progress,
            ct: ct
        );
    }

    #endregion

    // 将 Serilog 日志接入工具层日志回调
    private ChartLogSink CreateLogSink() =>
        new()
        {
            Info = msg => _log.Information(msg),
            Warning = msg => _log.Warning(msg),
            Error = msg => _log.Error(msg),
            Debug = msg => _log.Debug(msg),
        };
}
