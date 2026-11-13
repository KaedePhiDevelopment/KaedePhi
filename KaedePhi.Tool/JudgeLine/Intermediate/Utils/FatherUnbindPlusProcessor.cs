using System.Collections.Concurrent;
using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Event.Intermediate;
using KaedePhi.Tool.JudgeLine.Utils;

namespace KaedePhi.Tool.JudgeLine.Intermediate.Utils;

/// <summary>
/// IR 判定线父子解绑处理器（自适应采样）。
/// 以事件边界为强制切割点，仅在误差超过容差时插入新采样段，
/// 相较 <see cref="FatherUnbindProcessor"/> 可减少冗余段数。
/// </summary>
public class FatherUnbindPlusProcessor : FatherUnbindProcessorBase
{
    private readonly double _tolerance;
    private readonly double _mergeTolerance;
    private readonly EventListMergerPlus<double> _merger = new();

    /// <summary>
    /// 初始化父级解绑增强处理器的新实例。
    /// </summary>
    /// <param name="cache">共享缓存</param>
    /// <param name="tolerance">容差</param>
    /// <param name="mergeTolerance">事件通道合并容差百分比。</param>
    /// <param name="logInfo">信息日志回调</param>
    /// <param name="logWarning">警告日志回调</param>
    /// <param name="logError">错误日志回调</param>
    /// <param name="logDebug">调试日志回调</param>
    public FatherUnbindPlusProcessor(
        ConcurrentDictionary<JudgeLineUnbindCacheKey, Ir.JudgeLine> cache,
        double tolerance,
        double mergeTolerance,
        Action<string>? logInfo = null,
        Action<string>? logWarning = null,
        Action<string>? logError = null,
        Action<string>? logDebug = null
    )
        : base(cache, logInfo, logWarning, logError, logDebug)
    {
        _tolerance = tolerance;
        _mergeTolerance = mergeTolerance;
    }

    /// <summary>
    /// 自适应采样解绑：将判定线与父线解绑，以事件边界为强制切割点，
    /// 仅在误差超过容差时插入新采样段。相较 <see cref="FatherUnbindProcessor"/> 可减少冗余段数。
    /// </summary>
    public Ir.JudgeLine FatherUnbind(
        int targetJudgeLineIndex,
        List<Ir.JudgeLine> allJudgeLines,
        double precision,
        IProgress<ToolProgress>? progress = null,
        CancellationToken ct = default
    )
    {
        NumericParameterValidator.ValidatePrecision(precision);
        NumericParameterValidator.ValidateTolerance(_tolerance);
        NumericParameterValidator.ValidateTolerance(_mergeTolerance);
        ct.ThrowIfCancellationRequested();
        Ir.JudgeLine judgeLineCopy;
        try
        {
            progress?.Report(new ToolProgress(0.1, "准备解绑上下文"));

            var (judgeLine, fatherLine, shouldReturn) = PrepareUnbindContext(
                targetJudgeLineIndex,
                allJudgeLines,
                logTag: "FatherUnbindPlus",
                startAction: "开始解绑（自适应采样）",
                cacheKeyFactory: index => new JudgeLineUnbindCacheKey(
                    index,
                    precision,
                    _tolerance,
                    _mergeTolerance,
                    true,
                    JudgeLineUnbindContext.CurrentRenderProfile
                ),
                recursiveUnbind: (idx, lines) => FatherUnbind(idx, lines, precision, progress, ct)
            );

            judgeLineCopy = judgeLine;
            if (shouldReturn || fatherLine is null)
            {
                progress?.Report(new ToolProgress(1.0));
                return judgeLineCopy;
            }

            progress?.Report(new ToolProgress(0.2, "合并通道"));
            var ch = JudgeLineUnbindEvents.MergeChannels(
                judgeLineCopy.EventLayers,
                fatherLine.EventLayers,
                Merge
            );

            var rangeResult = JudgeLineAdaptiveSampler.TryGetOverallRange(ch);
            if (rangeResult is null)
            {
                judgeLineCopy.Father = -1;
                Cache.TryAdd(
                    new JudgeLineUnbindCacheKey(
                        targetJudgeLineIndex,
                        precision,
                        _tolerance,
                        _mergeTolerance,
                        true,
                        JudgeLineUnbindContext.CurrentRenderProfile
                    ),
                    judgeLineCopy
                );
                progress?.Report(new ToolProgress(1.0));
                return judgeLineCopy;
            }

            var (overallMin, overallMax) = rangeResult.Value;
            var step = new Beat(1d / precision);

            progress?.Report(new ToolProgress(0.4, "收集关键帧"));
            var keyBeats = JudgeLineAdaptiveSampler.CollectKeyBeats(overallMin, overallMax, ch);

            progress?.Report(new ToolProgress(0.6, "自适应采样"));
            LogDebug?.Invoke(
                $"FatherUnbindPlus[{targetJudgeLineIndex}]: 自适应采样，关键帧数={keyBeats.Count}，最大精度={precision}"
            );
            var (resultX, resultY) = JudgeLineAdaptiveSampler.Sample(
                keyBeats,
                step,
                _tolerance,
                ch,
                ct
            );

            progress?.Report(new ToolProgress(0.9, "写回结果"));
            LogDebug?.Invoke(
                $"FatherUnbindPlus[{targetJudgeLineIndex}]: 采样完成（生成 {resultX.Count} 段），写回结果"
            );
            JudgeLineUnbindLayerOperations.WriteResultToLine(
                judgeLineCopy,
                resultX,
                resultY,
                ch.Fr,
                Merge
            );

            Cache.TryAdd(
                new JudgeLineUnbindCacheKey(
                    targetJudgeLineIndex,
                    precision,
                    _tolerance,
                    _mergeTolerance,
                    true,
                    JudgeLineUnbindContext.CurrentRenderProfile
                ),
                judgeLineCopy
            );
            LogInfo?.Invoke($"FatherUnbindPlus[{targetJudgeLineIndex}]: 解绑完成");
            progress?.Report(new ToolProgress(1.0));
            return judgeLineCopy;

            List<IrEvents.Event<double>> Merge(
                List<IrEvents.Event<double>> a,
                List<IrEvents.Event<double>> b
            ) => _merger.EventListMerge(a, b, precision, _mergeTolerance);
        }
        catch (Exception ex)
        {
            LogError?.Invoke($"FatherUnbindPlus[{targetJudgeLineIndex}]: 解绑失败: " + ex.Message);
            throw;
        }
    }
}
