using KaedePhi.Core.Primitives;
using KaedePhi.Tool.Common;
using PhigrosEvent = KaedePhi.Core.Formats.Phigros.v3.Model.Event;
using PhigrosJudgeLine = KaedePhi.Core.Formats.Phigros.v3.Model.JudgeLine;
using PhigrosNoteType = KaedePhi.Core.Formats.Phigros.v3.Model.NoteType;
using PhigrosSpeedEvent = KaedePhi.Core.Formats.Phigros.v3.Model.SpeedEvent;

namespace KaedePhi.Tool.Converter.Phigros.v3.Utils;

/// <summary>
/// PhigrosV3 事件到 IR 事件的转换工具。
/// </summary>
public static class EventBuilder
{
    private const double TrailingBeatPadding = 1d / 64d;

    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <typeparam name="T">事件载荷类型</typeparam>
    /// <param name="events">待处理的事件列表</param>
    /// <param name="horizonBeat">范围结束拍位置</param>
    /// <param name="valueTransformer">转换输入值的函数</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static List<IrEvents.Event<T>>? ConvertEvents<T>(
        List<PhigrosEvent>? events,
        double horizonBeat,
        Func<float, T> valueTransformer
    )
        where T : notnull
    {
        if (events is not { Count: > 0 })
            return null;

        var sorted = events.OrderBy(e => e.StartTime).ToList();
        var result = new List<IrEvents.Event<T>>();

        foreach (var ev in sorted)
        {
            var startBeat = Math.Max(0d, ev.StartTime / 32.0);
            var endBeat = ev.EndTime / 32.0;
            if (endBeat <= startBeat)
                continue;

            var startValue = valueTransformer(ev.Start);
            var endValue = valueTransformer(ev.End);
            if (
                EqualityComparer<T>.Default.Equals(startValue, endValue)
                && endBeat - startBeat > 1d
            )
                endBeat = startBeat + 1d;
            result.Add(CreateLinearEvent(startBeat, endBeat, startValue, endValue));
        }

        if (result.Count == 0)
            return null;
        AddTrailingEvent(result, horizonBeat);
        return result;
    }

    /// <summary>
    /// 将移动事件转换为指定坐标轴的中间事件。
    /// </summary>
    /// <param name="events">待处理的事件列表。</param>
    /// <param name="horizonBeat">范围结束拍位置。</param>
    /// <param name="startSelector">选择事件起始值的函数。</param>
    /// <param name="endSelector">选择事件结束值的函数。</param>
    /// <param name="valueTransformer">转换事件数值的函数。</param>
    /// <returns>转换后的移动事件列表；没有有效事件时返回<see langword="null"/>。</returns>
    public static List<IrEvents.Event<double>>? ConvertMoveAxisEvents(
        List<PhigrosEvent>? events,
        double horizonBeat,
        Func<PhigrosEvent, float> startSelector,
        Func<PhigrosEvent, float> endSelector,
        Func<float, double> valueTransformer
    )
    {
        if (events is not { Count: > 0 })
            return null;

        var sorted = events.OrderBy(e => e.StartTime).ToList();
        var result = new List<IrEvents.Event<double>>();

        foreach (var ev in sorted)
        {
            var startBeat = Math.Max(0d, ev.StartTime / 32.0);
            var endBeat = ev.EndTime / 32.0;
            if (endBeat <= startBeat)
                continue;

            var startValue = valueTransformer(startSelector(ev));
            var endValue = valueTransformer(endSelector(ev));
            if (
                Math.Abs(startValue - endValue) < Constants.FloatEpsilon
                && endBeat - startBeat > 1d
            )
                endBeat = startBeat + 1d;
            result.Add(CreateLinearEvent(startBeat, endBeat, startValue, endValue));
        }

        if (result.Count == 0)
            return null;
        AddTrailingEvent(result, horizonBeat);
        return result;
    }

    /// <summary>
    /// 将源数据转换为目标格式的数据。
    /// </summary>
    /// <param name="events">待处理的事件列表</param>
    /// <param name="horizonBeat">范围结束拍位置</param>
    /// <returns>转换或构建得到的结果。</returns>
    public static List<IrEvents.Event<float>>? ConvertSpeedEvents(
        List<PhigrosSpeedEvent>? events,
        double horizonBeat
    )
    {
        if (events is not { Count: > 0 })
            return null;

        var sorted = events.OrderBy(e => e.StartTime).ToList();
        var result = new List<IrEvents.Event<float>>();

        foreach (var ev in sorted)
        {
            var startBeat = Math.Max(0d, ev.StartTime / 32.0);
            var endBeat = ev.EndTime / 32.0;
            var speedValue = (float)(ev.Value * Constants.SpeedValueRatio);
            if (endBeat <= startBeat)
                continue;

            if (endBeat - startBeat > 1d)
                endBeat = startBeat + 1d;
            result.Add(CreateLinearEvent(startBeat, endBeat, speedValue, speedValue));
        }

        if (result.Count == 0)
            return null;
        AddTrailingEvent(result, horizonBeat);
        return result;
    }

    /// <summary>
    /// 获取判定线水平拍。
    /// </summary>
    /// <param name="src">源对象</param>
    /// <returns>计算得到的数值。</returns>
    public static double GetJudgeLineHorizonBeat(PhigrosJudgeLine src)
    {
        var maxBeat = 0d;
        maxBeat = Math.Max(maxBeat, GetMaxBeat(src.NotesAbove.Select(n => (double)n.Time / 32)));
        maxBeat = Math.Max(maxBeat, GetMaxBeat(src.NotesBelow.Select(n => (double)n.Time / 32)));
        maxBeat = Math.Max(
            maxBeat,
            GetMaxBeat(
                src.NotesAbove.Where(n => n.Type == PhigrosNoteType.Hold)
                    .Select(n => (double)(n.Time + n.HoldTime) / 32)
            )
        );
        maxBeat = Math.Max(
            maxBeat,
            GetMaxBeat(
                src.NotesBelow.Where(n => n.Type == PhigrosNoteType.Hold)
                    .Select(n => (double)(n.Time + n.HoldTime) / 32)
            )
        );
        maxBeat = Math.Max(
            maxBeat,
            GetMaxBeat(
                src.SpeedEvents.Select(e =>
                    Math.Min((double)e.EndTime / 32, Math.Max(0d, (double)e.StartTime / 32) + 1)
                )
            )
        );
        maxBeat = Math.Max(
            maxBeat,
            GetMaxBeat(
                src.JudgeLineMoveEvents.Select(e =>
                    Math.Abs(e.Start - e.End) < Constants.FloatEpsilon
                        ? Math.Min(
                            (double)e.EndTime / 32,
                            Math.Max(0d, (double)e.StartTime / 32) + 1
                        )
                        : (double)e.EndTime / 32
                )
            )
        );
        maxBeat = Math.Max(
            maxBeat,
            GetMaxBeat(
                src.JudgeLineRotateEvents.Select(e =>
                    Math.Abs(e.Start - e.End) < Constants.FloatEpsilon
                        ? Math.Min(
                            (double)e.EndTime / 32,
                            Math.Max(0d, (double)e.StartTime / 32) + 1
                        )
                        : (double)e.EndTime / 32
                )
            )
        );
        maxBeat = Math.Max(
            maxBeat,
            GetMaxBeat(
                src.JudgeLineDisappearEvents.Select(e =>
                    Math.Abs(e.Start - e.End) < Constants.FloatEpsilon
                        ? Math.Min(
                            (double)e.EndTime / 32,
                            Math.Max(0d, (double)e.StartTime / 32) + 1
                        )
                        : (double)e.EndTime / 32
                )
            )
        );
        return maxBeat + TrailingBeatPadding;
    }

    private static IrEvents.Event<T> CreateLinearEvent<T>(
        double startBeat,
        double endBeat,
        T startValue,
        T endValue
    )
        where T : notnull =>
        new()
        {
            StartBeat = new Beat(startBeat),
            EndBeat = new Beat(endBeat),
            StartValue = startValue,
            EndValue = endValue,
        };

    private static void AddTrailingEvent<T>(List<IrEvents.Event<T>> events, double horizonBeat)
        where T : notnull
    {
        var lastEnd = (double)events[^1].EndBeat;
        var trailingEnd = Math.Max(horizonBeat, lastEnd + TrailingBeatPadding);
        if (trailingEnd <= lastEnd)
            return;

        events.Add(
            CreateLinearEvent(lastEnd, trailingEnd, events[^1].EndValue, events[^1].EndValue)
        );
    }

    private static double GetMaxBeat(IEnumerable<double>? beats) =>
        beats?.DefaultIfEmpty(0d).Max() ?? 0d;
}
