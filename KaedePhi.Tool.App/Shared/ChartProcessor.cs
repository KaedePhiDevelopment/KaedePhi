using KaedePhi.Core.Intermediate.Model.Events;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Event.Intermediate;
using KaedePhi.Tool.JudgeLines.Intermediate;
using KaedePhi.Tool.Layer.Intermediate;
using KaedePhi.Tool.Render.Intermediate;
using Chart = KaedePhi.Core.Intermediate.Model.Chart;

namespace KaedePhi.Tool.App.Shared;

public static class ChartProcessor
{
    public static void UnbindFather(
        Chart chart,
        double precision,
        double tolerance,
        bool classic,
        bool disableCompress = false,
        double mergeTolerance = Constants.DefaultTolerancePercent,
        ChartLogSink? log = null,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        NumericParameterValidator.ValidatePrecision(precision);
        NumericParameterValidator.ValidateTolerance(tolerance);
        NumericParameterValidator.ValidateTolerance(mergeTolerance);
        IrChartValidator.ValidateJudgeLineHierarchy(chart.JudgeLineList);

        var unbinder = new JudgeLineUnbinder();
        var compressor = new LayerProcessor();
        log?.AttachToAll(unbinder, compressor);

        var linesToProcess = new List<int>();
        for (var i = 0; i < chart.JudgeLineList.Count; i++)
        {
            if (chart.JudgeLineList[i].Father != -1)
                linesToProcess.Add(i);
        }

        var totalLines = linesToProcess.Count;
        for (var idx = 0; idx < totalLines; idx++)
        {
            ct.ThrowIfCancellationRequested();
            var i = linesToProcess[idx];
            var lineProgress = CreateLineProgress(progress, idx, totalLines);
            var unboundLine = classic
                ? unbinder.FatherUnbind(i, chart.JudgeLineList, precision, lineProgress, ct)
                : unbinder.FatherUnbindDynamic(
                    i,
                    chart.JudgeLineList,
                    precision,
                    tolerance,
                    mergeTolerance,
                    lineProgress,
                    ct
                );
            if (classic && !disableCompress)
            {
                foreach (var layer in unboundLine.EventLayers)
                    compressor.LayerEventsCompress(layer, tolerance, lineProgress);
            }

            chart.JudgeLineList[i] = unboundLine;
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    public static void LayerMerge(
        Chart chart,
        double precision,
        double tolerance,
        bool classic,
        bool disableCompress,
        ChartLogSink? log = null,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        NumericParameterValidator.ValidatePrecision(precision);
        NumericParameterValidator.ValidateTolerance(tolerance);

        var processor = new LayerProcessor();
        log?.AttachTo(processor);

        var totalLines = chart.JudgeLineList.Count;
        for (var li = 0; li < totalLines; li++)
        {
            ct.ThrowIfCancellationRequested();
            var line = chart.JudgeLineList[li];
            if (line.EventLayers is not { Count: > 1 })
            {
                ReportLineSkipped(progress, li, totalLines);
                continue;
            }

            var lineProgress = CreateLineProgress(progress, li, totalLines);
            var merged = classic
                ? processor.LayerMerge(line.EventLayers, precision, lineProgress)
                : processor.LayerMergePlus(line.EventLayers, precision, tolerance, lineProgress);
            if (!disableCompress)
                processor.LayerEventsCompress(merged, tolerance, lineProgress);
            line.EventLayers.Clear();
            line.EventLayers.Add(merged);
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    public static void CutEvent(
        Chart chart,
        double precision,
        double tolerance,
        bool disableCompress,
        ChartLogSink? log = null,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        NumericParameterValidator.ValidatePrecision(precision);
        NumericParameterValidator.ValidateTolerance(tolerance);

        var processor = new LayerProcessor();
        log?.AttachTo(processor);

        var totalLines = chart.JudgeLineList.Count;
        for (var li = 0; li < totalLines; li++)
        {
            ct.ThrowIfCancellationRequested();
            var line = chart.JudgeLineList[li];
            if (line.EventLayers is not { Count: > 0 })
            {
                ReportLineSkipped(progress, li, totalLines);
                continue;
            }

            var lineProgress = CreateLineProgress(progress, li, totalLines);
            line.EventLayers = processor.CutLayerEvents(line.EventLayers, precision, lineProgress);
            if (disableCompress)
                continue;
            foreach (var layer in line.EventLayers)
                processor.LayerEventsCompress(layer, tolerance, lineProgress);
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    public static void FitEvent(
        Chart chart,
        double tolerance,
        ChartLogSink? log = null,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        NumericParameterValidator.ValidateTolerance(tolerance);

        var doubleFit = new EventFit<double>();
        var intFit = new EventFit<int>();
        var floatFit = new EventFit<float>();
        log?.AttachToAll(doubleFit, intFit, floatFit);

        var totalLines = chart.JudgeLineList.Count;
        for (var li = 0; li < totalLines; li++)
        {
            ct.ThrowIfCancellationRequested();
            var line = chart.JudgeLineList[li];
            if (line.EventLayers is not { Count: > 0 })
            {
                ReportLineSkipped(progress, li, totalLines);
                continue;
            }

            var totalLayers = line.EventLayers.Count;
            for (var ei = 0; ei < totalLayers; ei++)
            {
                ct.ThrowIfCancellationRequested();
                var capturedLi = li;
                var capturedEi = ei;
                var layerProgress = progress is null
                    ? null
                    : new Progress<ToolProgress>(p =>
                    {
                        var overall = (capturedLi + (double)capturedEi / totalLayers) / totalLines;
                        progress.Report(new ToolProgress(p.Percentage, overall, p.Detail));
                    });
                FitLayer(
                    line.EventLayers[ei],
                    doubleFit,
                    intFit,
                    floatFit,
                    tolerance,
                    layerProgress
                );
            }
        }

        progress?.Report(new ToolProgress(1.0, 1.0));
    }

    public static IReadOnlyList<string> Render(
        Chart chart,
        string outputDir,
        IrRenderOptions options,
        int? lineIndex = null,
        int? layerIndex = null,
        ChartLogSink? log = null,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        IrRenderValidator.Validate(chart, options, lineIndex, layerIndex);
        var exporter = new IrChartRenderExporter();
        log?.AttachTo(exporter);
        return exporter.ExportChart(chart, outputDir, options, lineIndex, layerIndex, progress, ct);
    }

    private static void FitLayer(
        EventLayer layer,
        EventFit<double> doubleFit,
        EventFit<int> intFit,
        EventFit<float> floatFit,
        double tolerance,
        IProgress<ToolProgress>? progress
    )
    {
        if (layer.MoveXEvents is { Count: > 0 })
            layer.MoveXEvents = doubleFit.FitEvents(layer.MoveXEvents, tolerance);
        if (layer.MoveYEvents is { Count: > 0 })
            layer.MoveYEvents = doubleFit.FitEvents(layer.MoveYEvents, tolerance);
        if (layer.RotateEvents is { Count: > 0 })
            layer.RotateEvents = doubleFit.FitEvents(layer.RotateEvents, tolerance);
        if (layer.AlphaEvents is { Count: > 0 })
            layer.AlphaEvents = intFit.FitEvents(layer.AlphaEvents, tolerance);
        if (layer.SpeedEvents is { Count: > 0 })
            layer.SpeedEvents = floatFit.FitEvents(layer.SpeedEvents, tolerance);
        progress?.Report(new ToolProgress(1.0));
    }

    // 将单条判定线内的进度折算为整体进度
    private static IProgress<ToolProgress>? CreateLineProgress(
        IProgress<ToolProgress>? progress,
        int lineIndex,
        int totalLines
    ) =>
        progress is null
            ? null
            : new Progress<ToolProgress>(p =>
                progress.Report(
                    new ToolProgress(
                        p.Percentage,
                        (lineIndex + p.Percentage) / totalLines,
                        p.Detail
                    )
                )
            );

    private static void ReportLineSkipped(
        IProgress<ToolProgress>? progress,
        int lineIndex,
        int totalLines
    ) => progress?.Report(new ToolProgress(1.0, (double)(lineIndex + 1) / totalLines));
}
